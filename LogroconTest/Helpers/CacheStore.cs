using System;
using System.Collections.Generic;
using System.Threading;
using LogroconTest.Models;

namespace LogroconTest.Helpers
{
    public interface ICacheStore
    {
        OfficerData GetOfficer(int id, string sessionn);

        void AddOfficer(int id, OfficerData value, string session);

        void RemoveOfficer(int id, string session);

        void EditOfficer(int id, OfficerDataIn value, string session);
        
        PostData GetPost(int Id, string session);

        void AddPost(int Id, PostData value, string session);

        void RemovePost(int Id, string session);

        void EditPost(int Id, PostDataIn value, string session);
    }

    public class CacheStore : ICacheStore
    {
        private Dictionary<int, OfficerData> _officer;
        private Dictionary<int, PostData> _posts;
        
        private ReaderWriterLockSlim _locker;
        private ReaderWriterLockSlim _lockerPost;

        public CacheStore(bool CachedOff, bool CachedPost)
        {
            if (CachedOff)
            {
                _officer = new Dictionary<int, OfficerData>();
                _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            }

            if (CachedPost)
            {
                _posts = new Dictionary<int, PostData>();
                _lockerPost = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            }
        }

        /// <summary>
        /// Получить информацию о сотруднике по ID из кэша
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sessionn"></param>
        /// <returns></returns>
        public OfficerData GetOfficer(int id, string sessionn)
        {
            _locker.EnterReadLock();
            try
            {
                if (_officer.ContainsKey(id))
                {
                    return _officer[id];
                }

                return null;
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        /// <summary>
        /// Добавление информации о сотруднике в кэш
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="session"></param>
        public void AddOfficer(int id, OfficerData value, string session)
        {
            if (value == null || _officer.ContainsKey(id))
                return;

            _locker.EnterWriteLock();

            try
            {
                value.ID = id;
                _officer.Add(id, value);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Удаление информации о сотруднике из кэша
        /// </summary>
        /// <param name="id"></param>
        /// <param name="session"></param>
        public void RemoveOfficer(int id, string session)
        {
            if (!_officer.ContainsKey(id))
                return;

            _locker.EnterWriteLock();
            try
            {
                _officer.Remove(id);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Изменение информации о должности по ID в кэше
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="session"></param>
        public void EditOfficer(int id, OfficerDataIn value, string session)
        {
            if (value == null || !_officer.ContainsKey(id))
                return;

            _locker.EnterWriteLock();
            try
            {
                var officerData = new OfficerData(value);
                
                RemoveOfficer(id, session);
                AddOfficer(id, officerData, session);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Получение информации об должности по ID из кэша
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public PostData GetPost(int Id, string session)
        {
            if (!_posts.ContainsKey(Id))
                return null;
        
            _lockerPost.EnterReadLock();
            try
            {
                return _posts[Id];
            }
            finally
            {
                _lockerPost.ExitReadLock();
            }
        }

        public void AddPost(int Id, PostData value, string session)
        {
            if (value == null || _posts.ContainsKey(Id))
                return;

            _lockerPost.EnterWriteLock();
            try
            {
                value.ID = Id;
                _posts.Add(Id, value);
            }
            finally
            {
                _lockerPost.ExitWriteLock();
            }
        }

        public void RemovePost(int Id, string session)
        {
            if (!_posts.ContainsKey(Id))
                return;

            _lockerPost.EnterWriteLock();
            try
            {
                _posts.Remove(Id);
            }
            finally
            {
                _lockerPost.ExitWriteLock();
            }
        }

        public void EditPost(int Id, PostDataIn value, string session)
        {
            if (value == null || !_posts.ContainsKey(Id))
                return;

            _lockerPost.EnterWriteLock();
            try
            {
                if (!string.IsNullOrWhiteSpace(value.PostsName))
                    _posts[Id].PostsName = value.PostsName;

                if (value.Grade > 0)
                    _posts[Id].Grade = value.Grade;
            }
            finally
            {
                _lockerPost.ExitWriteLock();
            }
        }
        
    }
}
