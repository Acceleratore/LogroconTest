using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LogroconTest.Models
{
    /// <summary>
    /// Ответ в случае отсутствия данных
    /// </summary>
    public class ModelResponse
    {
        /// <summary>
        /// Номер сессии
        /// </summary>
        public string Session { get; set; }
        
        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; set; }
    }
}
