using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LogroconTest.Helpers;
using LogroconTest.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace LogroconTest.Controllers
{
    /// <summary>
    /// API для должностей сотрудников
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PostInfoController : ControllerBase
    {
        ModelDataBase workdb;

        public PostInfoController(IOptions<Settings> setting, ICacheStore cache)
        {
            workdb = new ModelDataBase(setting, cache);
        }

        /// <summary>
        /// Получение списка должностей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<List<PostData>> Get()
        {
            var session = Guid.NewGuid().ToString();

            var result = workdb.GetPosts(session);
            if (result == null || result.Count == 0)
                return NoContent();

            return Ok(result);
        }

        /// <summary>
        /// Получение информации о должности по ID
        /// </summary>
        /// <param name="id">Id должности</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetPostByID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<PostData> GetPostByID(int id)
        {
            var session = Guid.NewGuid().ToString();
            
            if (id < 0)
                return BadRequest(Utils.GetResponse(session, "Id должен быть положительным"));
            
            var result = workdb.GetPost(id, session);
            
            if (result == null)
                return NotFound(Utils.GetResponse(session));
            
            return Ok(result);
        }

        /// <summary>
        /// Добавление должности
        /// </summary>
        /// <param name="value">Данные для добавления должности</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Post([FromBody] PostDataIn value)
        {
            var session = Guid.NewGuid().ToString();

            if (value == null || string.IsNullOrWhiteSpace(value.PostsName) || (value.Grade < 1 || value.Grade > 15))
                return BadRequest(Utils.GetResponse(session, "Неверные данные"));

            var result = workdb.CreatePost(value, session);

            return CreatedAtAction(nameof(GetPostByID), new { id = result }, result);
        }

        /// <summary>
        /// Обновление должности
        /// </summary>
        /// <param name="id">id обновляемой должности</param>
        /// <param name="value">Новые данные должности</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Put(int id, [FromBody] PostDataIn value)
        {
            var session = Guid.NewGuid().ToString();

            if (value == null || string.IsNullOrWhiteSpace(value.PostsName) || id < 0 || (value.Grade < 1 || value.Grade > 15))
                return BadRequest(Utils.GetResponse(session, "Неверные данные"));

            var result = workdb.UpdatePost(id, value, session);
            if (!result)
                return NotFound(Utils.GetResponse(session, "Нет объектов для обновления"));

            return Ok(Utils.GetResponse(session));
        }

        /// <summary>
        /// Удаление должности
        /// </summary>
        /// <param name="id">Id должности</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(int id)
        {
            var session = Guid.NewGuid().ToString();

            var post = workdb.GetPost(id, session);
            if (post == null)
                NotFound(Utils.GetResponse(session));

            // Имеется связь с сотрудником, удаление невозможно
            var link = workdb.GetLinkOfficerPosts(id, session);
            if (link != null && link.Count > 0)
                return Conflict(Utils.GetResponse(session, string.Format("Данная должность имеет связь с сотрудниками ID = {0}. Удаление невозможно, необходимо убрать связь указанных сотрудников с должностью.", string.Join(',', link.ToArray())))); // Не уверен, что код верный

            workdb.DeletePost(id, session);

            return Ok(Utils.GetResponse(session));
        }
    }
}
