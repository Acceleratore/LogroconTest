using System;
using System.Collections.Generic;
using System.Linq;
using LogroconTest.Helpers;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LogroconTest.Models
{
    public class ModelDataBase
    {
        private DBSettings MainConnection;
        ICacheStore _cache;
        IOptions<Settings> _setting;

        public ModelDataBase(IOptions<Settings> setting, ICacheStore cache)
        {
            MainConnection = setting.Value.MainDBConnection;
            _setting = setting;
            _cache = cache;
        }

        /// <summary>
        /// Получение списка сотрудников
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public List<OfficerData> GetOfficers(string session)
        {
            var result = new List<OfficerData>();

            try
            {
                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();

                    var sqlQuery = string.Format(@"select ID, FirstName, Surname, Patronymic,  BirthDate
                                                     from {0}Officer", MainConnection.GetSQLNamespace());

                    using (var _postgreCommand = new NpgsqlCommand(sqlQuery, _connection))
                    using (var _reader = _postgreCommand.ExecuteReader())
                    {
                        if (_reader.Read())
                        {
                            var officer = new OfficerData();

                            officer.ID         = Convert.ToInt32(_reader["ID"]);
                            officer.Name       = _reader["FirstName"].ToString();
                            officer.SurName    = _reader["Surname"].ToString();
                            officer.Patronymic = _reader["Patronymic"].ToString();
                            officer.BirthDate  = _reader["BirthDate"] == null ? DateTime.MinValue : Convert.ToDateTime(_reader["BirthDate"]);

                            officer.Posts = GetPostsInfoByOfficerID(Convert.ToInt32(_reader["ID"]), session);

                            result.Add(officer);
                        }
                    }
                }
            } catch (Exception e)
            {
                throw new Exception("Ошибка получения списка сотрудников.", e);
            }

            return result;
        }

        /// <summary>
        /// Получение информации о сотруднике
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public OfficerData GetOfficerInfoByID(int ID, string session)
        {
            OfficerData result = null;

            try
            {

                if (_setting.Value.ChachedEmployee)
                {
                    result = _cache.GetOfficer(ID, session);
                }

                if (result != null)
                    return result;

                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();

                    var sqlQuery = string.Format(@"select ID, FirstName, Surname, Patronymic,  BirthDate
                                                 from {0}Officer
                                                where ID = @id", MainConnection.GetSQLNamespace());

                    using (var _postgreCommand = new NpgsqlCommand(sqlQuery, _connection))
                    {
                        _postgreCommand.Parameters.AddWithValue("id", ID);

                        using (var _reader = _postgreCommand.ExecuteReader())
                        {
                            if (_reader.Read())
                            {
                                result = new OfficerData();

                                result.ID         = ID;
                                result.Name       = _reader["FirstName"].ToString();
                                result.SurName    = _reader["Surname"].ToString();
                                result.Patronymic = _reader["Patronymic"].ToString();
                                result.BirthDate  = _reader["BirthDate"] == null ? DateTime.MinValue : Convert.ToDateTime(_reader["BirthDate"]);

                                result.Posts = GetPostsInfoByOfficerID(ID, session);
                            }
                        }
                    }
                }

                if (_setting.Value.ChachedEmployee)
                {
                    _cache.AddOfficer(ID, result, session);
                }
            } catch (Exception e)
            {
                throw new Exception("Ошибка получения информации о сотрудники с ID = " + ID, e);
            }

            return result;
        }
        
        /// <summary>
        /// Создания информации о сотруднике
        /// </summary>
        /// <param name="data"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public OfficerData CreateOfficerInfo(OfficerDataIn data, string session)
        {
            var id = -1;
            var result = new OfficerData(data);
            var posts = new List<PostData>();

            using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
            {
                _connection.Open();
                
                var _transaction =_connection.BeginTransaction();

                try
                {
                    var sqlQueryOfficer = string.Format(@"INSERT INTO {0}Officer (FirstName, Surname, Patronymic, BirthDate)
                                                               VALUES (@firstname, @surname, @patronymic, @birthdate) RETURNING ID", MainConnection.GetSQLNamespace());
                    
                    using (var _postgreCommand = new NpgsqlCommand(sqlQueryOfficer, _connection))
                    {
                        _postgreCommand.Parameters.AddWithValue("firstname",  data.Name);
                        _postgreCommand.Parameters.AddWithValue("surname",    Utils.NoNullValue(data.SurName));
                        _postgreCommand.Parameters.AddWithValue("patronymic", Utils.NoNullValue(data.Patronymic));
                        _postgreCommand.Parameters.AddWithValue("birthdate",  data.BirthDate);

                        id = Convert.ToInt32(_postgreCommand.ExecuteScalar());
                    }
                    
                    foreach (var post in data.Posts)
                    {
                        CreateOfficerPost(id, post.ID, session, _connection);
                        posts.Add(GetPost(post.ID, session));
                    }

                    _transaction.Commit();

                    result.ID    = id;
                    result.Posts = posts;

                    if (_setting.Value.ChachedEmployee)
                    {
                        _cache.AddOfficer(id, result, session);
                    }
                }
                catch(Exception e)
                {
                    _transaction.Rollback();
                    throw new Exception("Ошибка добавления сотрудника.", e);
                }
            }

            return result;
        }

        /// <summary>
        /// Добавление должности сотруднику по Id
        /// </summary>
        /// <param name="id_off"></param>
        /// <param name="id_post"></param>
        /// <param name="session"></param>
        /// <param name="_connection"></param>
        /// <returns></returns>
        public int CreateOfficerPost(int id_off, int id_post, string session, NpgsqlConnection _connection)
        {
            var result = -1;
            var sqlQueryPosts = string.Format(@"INSERT INTO {0}Officer_to_posts (ID_Officer, ID_Post) VALUES (@id_off, @id_post)", MainConnection.GetSQLNamespace());

            try
            {
                using (var _postgreCommand = new NpgsqlCommand(sqlQueryPosts, _connection))
                {
                    _postgreCommand.Parameters.AddWithValue("id_off", id_off);
                    _postgreCommand.Parameters.AddWithValue("id_post", id_post);

                    _postgreCommand.ExecuteNonQuery();
                }

            } catch (Exception e)
            {
                throw new Exception("Ошибка добавления должности для сотрудника с ID = " + id_post, e);
            }

            return result;
        }

        /// <summary>
        /// Добавление должности сотруднику по Id
        /// </summary>
        /// <param name="id_off"></param>
        /// <param name="id_post"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public int CreateOfficerPost(int id_off, int id_post, string session)
        {
            var result = -1;
            
            using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
            {
                _connection.Open();

                result = CreateOfficerPost(id_off, id_post, session, _connection);
            }

            return result;
        }

        /// <summary>
        /// Обновление информации о сотруднике
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="session"></param>
        public void UpdateOfficer(int id, OfficerDataIn data, string session)
        {
            using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
            {
                _connection.Open();

                var _transaction = _connection.BeginTransaction();

                try
                {
                    var sqlQueryOfficer = string.Format(@"UPDATE {0}officer SET firstname = @firstname, surname = @surname, patronymic = @patronymic, birthdate = @birthdate
                                                           WHERE id = @id RETURNING ID", MainConnection.GetSQLNamespace());

                    using (var _postgreCommand = new NpgsqlCommand(sqlQueryOfficer, _connection))
                    {
                        _postgreCommand.Parameters.AddWithValue("id",         id);
                        _postgreCommand.Parameters.AddWithValue("firstname",  data.Name);
                        _postgreCommand.Parameters.AddWithValue("surname",    Utils.NoNullValue(data.SurName));
                        _postgreCommand.Parameters.AddWithValue("patronymic", Utils.NoNullValue(data.Patronymic));
                        _postgreCommand.Parameters.AddWithValue("birthdate",  data.BirthDate);

                        _postgreCommand.ExecuteNonQuery();
                    }

                    DeleteLink(id, session, _connection);

                    if (data.Posts != null && data.Posts.Count() > 0)
                    {
                        foreach (var post in data.Posts)
                        {
                            CreateOfficerPost(id, post.ID, session, _connection);
                        }
                    }

                    _transaction.Commit();

                    if (_setting.Value.ChachedEmployee)
                    {
                        _cache.EditOfficer(id, data, session);
                    }
                }
                catch (Exception e)
                {
                    _transaction.Rollback();
                    throw new Exception("Ошибка обновления данных сотрудника.", e);
                }

            }
        }

        /// <summary>
        /// Очистка должностей сотрудника
        /// </summary>
        /// <param name="id">Id сотрудника</param>
        /// <param name="session"></param>
        /// <param name="_connection"></param>
        public void DeleteLink(int id, string session, NpgsqlConnection _connection)
        {
            try
            {
                var sqlQuery = string.Format(@"delete from {0}officer_to_posts where id_officer = @id", MainConnection.GetSQLNamespace());

                using (var _postgreCommand = new NpgsqlCommand(sqlQuery, _connection))
                {
                    _postgreCommand.Parameters.AddWithValue("id", id);
                    _postgreCommand.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка удаления должностей сотрудника с ID = " + id, e);
            }
        }
        
        /// <summary>
        /// Удаление сотрудника по Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="session"></param>
        public void DeleteOfficer(int id, string session)
        {
            try
            {
                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();
                    var _transaction = _connection.BeginTransaction();

                    try
                    {
                        DeleteLink(id, session, _connection);

                        var sqlQuery = string.Format(@"delete from {0}officer where ID = @id", MainConnection.GetSQLNamespace());

                        using (var _postgreCommand = new NpgsqlCommand(sqlQuery, _connection))
                        {
                            _postgreCommand.Parameters.AddWithValue("id", id);
                            _postgreCommand.ExecuteNonQuery();
                        }

                        _transaction.Commit();

                        if (_setting.Value.ChachedEmployee)
                        {
                            _cache.RemoveOfficer(id, session);
                        }

                    }
                    catch (Exception e)
                    {
                        _transaction.Rollback();
                        throw new Exception("Ошибка удаления сотрудника с ID = " + id, e);
                    }
                }
            } catch (Exception e)
            {
                throw new Exception("Ошибка удаления сотрудника с ID = " + id, e);
            }
        }

        /// <summary>
        /// Получить информацию о должностях сотрудника по Id сотрудника
        /// </summary>
        /// <param name="id"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public List<PostData> GetPostsInfoByOfficerID(int id, string session)
        {
            var result = new List<PostData>();

            try
            {
                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();

                    var sqlQuery = string.Format(@"select posts.ID, posts.Namepost, posts.grade
                                                     from {0}Posts posts
                                                     join logrocon.Officer_to_posts as link ON link.ID_post = posts.ID
                                                    where link.ID_Officer = @id", MainConnection.GetSQLNamespace());

                    using (var _postgreCommand = new NpgsqlCommand(sqlQuery, _connection))
                    {
                        _postgreCommand.Parameters.AddWithValue("id", id);

                        using (var _reader = _postgreCommand.ExecuteReader())
                        {
                            while (_reader.Read())
                            {
                                var post = new PostData();

                                post.ID        = Convert.ToInt32(_reader["ID"]);
                                post.PostsName = _reader["Namepost"].ToString();
                                post.Grade     = Convert.ToInt32(_reader["grade"]);
                                
                                if (_setting.Value.ChachedPosts)
                                {
                                    _cache.AddPost(post.ID, post, session);
                                }

                                result.Add(post);
                            }
                        }
                    }
                }

            } catch (Exception e)
            {
                throw new Exception("Ошибка получения должностей сотрудника с ID = " + id, e);
            }

            return result.Count == 0 ? null : result;
        }

        /// <summary>
        /// Получить все должности
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public List<PostData> GetPosts(string session)
        {
            var result = new List<PostData>();

            try
            {
                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();

                    var sqlQuery = string.Format(@"SELECT id, namepost, grade
                                                 FROM {0}posts", MainConnection.GetSQLNamespace());

                    using (var _postgreCommand = new NpgsqlCommand(sqlQuery, _connection))
                    {
                        using (var _reader = _postgreCommand.ExecuteReader())
                        {
                            while (_reader.Read())
                            {
                                var post = new PostData();

                                post.ID        = Convert.ToInt32(_reader["id"]);
                                post.PostsName = _reader["namepost"].ToString();
                                post.Grade     = Convert.ToInt32(_reader["grade"]);

                                result.Add(post);
                            }
                        }
                    }
                }
            } catch (Exception e)
            {
                throw new Exception("Ошибка получения списка должностей ", e);
            }
            return result;
        }

        /// <summary>
        /// Получить должность по Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public PostData GetPost(int id, string session)
        {
            PostData result = null;

            try
            {
                if (_setting.Value.ChachedPosts)
                {
                    result = _cache.GetPost(id, session);
                }

                if (result != null)
                    return result;

                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();

                    var sqlQuery = string.Format(@"SELECT namepost, grade
                                                 FROM {0}posts
                                                WHERE id = @id", MainConnection.GetSQLNamespace());

                    using (var _postgreCommand = new NpgsqlCommand(sqlQuery, _connection))
                    {
                        _postgreCommand.Parameters.AddWithValue("id", id);

                        using (var _reader = _postgreCommand.ExecuteReader())
                        {
                            if (_reader.Read())
                            {
                                result = new PostData();

                                result.ID        = id;
                                result.PostsName = _reader["namepost"].ToString();
                                result.Grade     = Convert.ToInt32(_reader["grade"]);
                            }
                        }
                    }
                }

                if (_setting.Value.ChachedPosts)
                {
                    _cache.AddPost(id, result, session);
                }

            } catch (Exception e)
            {
                throw new Exception("Ошибка получения должности с ID = " + id, e);
            }

            return result;
        }

        /// <summary>
        /// Обновление информации о должности по Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool UpdatePost(int id, PostDataIn data, string session)
        {
            var result = false;
            var flagname  = !string.IsNullOrWhiteSpace(data.PostsName);
            var flaggrade = data.Grade != 0;

            try
            {
                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();

                    var sqlValue = string.Format("{0}{2}{1}",
                                                  flagname ? "namepost = @name" : string.Empty,
                                                  flaggrade ? "grade = @grade" : string.Empty,
                                                  flagname && flaggrade ? "," : string.Empty);

                    var sqlQueryPosts = string.Format(@"UPDATE {0}posts SET {1}
                                                         WHERE id = @id",
                                                        MainConnection.GetSQLNamespace(), sqlValue);

                    using (var _postgreCommand = new NpgsqlCommand(sqlQueryPosts, _connection))
                    {
                        _postgreCommand.Parameters.AddWithValue("id", id);

                        if (flagname)
                            _postgreCommand.Parameters.AddWithValue("name", data.PostsName);

                        if (flaggrade)
                            _postgreCommand.Parameters.AddWithValue("grade", data.Grade);

                        result = _postgreCommand.ExecuteReader().HasRows;
                    }

                    if (_setting.Value.ChachedPosts)
                    {
                        _cache.EditPost(id, data, session);
                    }
                }
            } catch (Exception e)
            {
                throw new Exception("Ошибка обновления должности с ID = " + id, e);
            }

            return result;
        }

        /// <summary>
        /// Создание должности
        /// </summary>
        /// <param name="data"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public PostData CreatePost(PostDataIn data, string session)
        {
            var id     = -1;
            var result = new PostData(data);

            try
            {
                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();

                    var sqlQueryOfficer = string.Format(@"INSERT INTO {0}posts (namepost, grade) VALUES (@name, @grade) RETURNING ID", MainConnection.GetSQLNamespace());

                    using (var _postgreCommand = new NpgsqlCommand(sqlQueryOfficer, _connection))
                    {
                        _postgreCommand.Parameters.AddWithValue("name", data.PostsName);
                        _postgreCommand.Parameters.AddWithValue("grade", data.Grade);

                        id = Convert.ToInt32(_postgreCommand.ExecuteScalar());
                    }
                }

                result.ID = id;
                if (_setting.Value.ChachedPosts)
                {
                    _cache.AddPost(id, result, session);
                }

            } catch (Exception e)
            {
                throw new Exception("Ошибка изменения должности с ID = " + id, e);
            }

            return result;
            ;
        }

        /// <summary>
        /// Удаление должности
        /// </summary>
        /// <param name="id"></param>
        /// <param name="session"></param>
        public void DeletePost(int id, string session)
        {
            try
            {
                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();

                    var sqlQuery = string.Format(@"DELETE FROM {0}posts WHERE id = @id", MainConnection.GetSQLNamespace());

                    using (var _postgreCommand = new NpgsqlCommand(sqlQuery, _connection))
                    {
                        _postgreCommand.Parameters.AddWithValue("id", id);
                        _postgreCommand.ExecuteNonQuery();
                    }
                }

                if (_setting.Value.ChachedPosts)
                {
                    _cache.RemovePost(id, session);
                }

            }
            catch (Exception e)
            {
                throw new Exception("Ошибка должности с ID = " + id, e);
            }
        }

        /// <summary>
        /// Список сотрудников связанной с должностью
        /// </summary>
        /// <param name="id">Id должности</param>
        /// <param name="session"></param>
        /// <returns></returns>
        public List<int> GetLinkOfficerPosts(int id, string session)
        {
            var result = new List<int>();
            try
            {
                using (var _connection = new NpgsqlConnection(MainConnection.GetConnectionString()))
                {
                    _connection.Open();

                    var sqlQuery = string.Format(@"SELECT id_officer
                                                 FROM {0}officer_to_posts
                                                WHERE id_post = @id", MainConnection.GetSQLNamespace());

                    using (var _postgreCommand = new NpgsqlCommand(sqlQuery, _connection))
                    {
                        _postgreCommand.Parameters.AddWithValue("id", id);

                        using (var _reader = _postgreCommand.ExecuteReader())
                        {
                            while (_reader.Read())
                            {
                                result.Add(Convert.ToInt32(_reader["id_officer"]));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка получения списка сотрудников для должности с ID = " + id, e);
            }

            return result;
        }
    }
}
