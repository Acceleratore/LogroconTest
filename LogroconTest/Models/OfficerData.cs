using System;
using System.Collections.Generic;

namespace LogroconTest.Models
{
    /// <summary>
    /// Данные должности для создания
    /// </summary>
    public class PostDataIn
    {
        /// <summary>
        /// Наименование должности
        /// </summary>
        public string PostsName { get; set; }
        
        /// <summary>
        /// Грейд должности
        /// </summary>
        public int Grade { get; set; }
    }

    /// <summary>
    /// Данные должности для просмотра
    /// </summary>
    public class PostData : PostDataIn
    {
        /// <summary>
        /// Id должности
        /// </summary>
        public int ID { get; set; } = -1;
    }
    
    /// <summary>
    /// Данные сотрудника для создания
    /// </summary>
    public class OfficerDataIn
    {
        /// <summary>
        /// Имя сотрудника
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Фамилия сотрудника
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// Отчество сотрудника
        /// </summary>
        public string Patronymic { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Список должностей
        /// </summary>
        public List<PostData> Posts { get; set; }
    }

    /// <summary>
    /// Полные данные о сотруднике и его должностях (для просмотра)
    /// </summary>
    public class OfficerData : OfficerDataIn
    {
        /// <summary>
        /// Id сотрудника
        /// </summary>
        public int ID { get; set; } = -1;
    }
    
    
}
