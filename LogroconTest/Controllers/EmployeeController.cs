using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LogroconTest.Helpers;
using LogroconTest.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace LogroconTest.Controllers
{
    /// <summary>
    /// API для работы с данными сотрудников
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        ModelDataBase workdb;

        public EmployeeController(IOptions<Settings> setting)
        {
            workdb = new ModelDataBase(setting);
        }
        
        /// <summary>
        /// Получение списка сотрудников
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<OfficerData> Get()
        {
            var session = Guid.NewGuid().ToString();
            
            var result = workdb.GetOfficers(session);

            if (result == null || result.Count() <= 0)
                return NotFound(Utils.GetResponse(session));

            return Ok(result);
        }

        /// <summary>
        /// Получение информации о сотруднике по ID
        /// </summary>
        /// <param name="id">ID сотрудника, положительное число</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<OfficerData> GetOfficerByID(int id)
        {
            var session = Guid.NewGuid().ToString();

            if (id < 0)
                return BadRequest(Utils.GetResponse(session, "Id должен быть положительным числом"));

            var result = workdb.GetOfficerInfoByID(id, session);

            if (result == null || result.ID < 0)
                return NotFound(Utils.GetResponse(session));

            return Ok(result);
        }

        /// <summary>
        /// Добавление нового сотрудника
        /// </summary>
        /// <param name="value">Данные для создания сотрудника</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult PostEmployee([FromBody] OfficerDataIn value)
        {
            var session = Guid.NewGuid().ToString();

            if (value == null || string.IsNullOrWhiteSpace(value.Name))
                return BadRequest(Utils.GetResponse(session));
            
            var result   = workdb.CreateOfficerInfo(value, session);
            var outValue = workdb.GetOfficerInfoByID(result, session);

            return CreatedAtAction(nameof(GetOfficerByID), new { id = result }, outValue);
        }

        /// <summary>
        /// Обновление информации о сотруднике
        /// </summary>
        /// <param name="id">Id сотрудника</param>
        /// <param name="value">Данные на которые обновить сотрудника</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Put(int id, [FromBody] OfficerDataIn value)
        {
            var session = Guid.NewGuid().ToString();

            if (value == null || id < 0 || string.IsNullOrWhiteSpace(value.Name))
                return BadRequest(Utils.GetResponse(session));

            var off = workdb.GetOfficerInfoByID(id, session);
            if (off == null || off.ID < 0)
                return NotFound(Utils.GetResponse(session));

            workdb.UpdateOfficer(id, value, session);
            
            return Ok(Utils.GetResponse(session));
        }
        
        /// <summary>
        /// Удаление сотрудника
        /// </summary>
        /// <param name="id">Id сотрудника</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(int id)
        {
            var session = Guid.NewGuid().ToString();

            var officer = workdb.GetOfficerInfoByID(id, session);
            if (officer == null || officer.ID < 0)
                NotFound(Utils.GetResponse(session));
            
            workdb.DeleteOfficer(id, session);

            return Ok(Utils.GetResponse(session));
        }
    }
}
