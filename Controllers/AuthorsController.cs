using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStoreAPI.Contract;
using BookStoreAPI.Data;
using BookStoreAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreAPI.Controllers
{
    /// <summary>
    /// Endpoint for book store's API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IloggerService _logger;
        private readonly IMapper _mapper;
        public AuthorsController(IAuthorRepository authorRepository, IloggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// Get all authors without filtering
        /// </summary>
        /// <returns>List of authors in the system</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Try to get all authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Success to get all authors");
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogInfo($"{e.Message} - {e.InnerException}");
                return StatusCode(500, "Internal Error");
            }
            

        }
        /// <summary>
        /// Get author by id
        /// </summary>
        /// <returns>Autor filtered by Id</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo("Try to get author");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo("Success to author");
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogInfo($"{e.Message} - {e.InnerException}");
                return StatusCode(500, "Internal Error");
            }
        }
        /// <summary>
        /// Create new entity of Author
        /// </summary>
        /// <param name="createAuthorDTO">FirstName,LastName,Bio</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create ([FromBody] CreateAuthorDTO createAuthorDTO)
        {
            try
            {
                if (createAuthorDTO == null)
                {
                    _logger.LogWarn("Bed request from Create0");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Bed request from Create1");
                    return BadRequest(ModelState);
                }
                var record = _mapper.Map<Author>(createAuthorDTO);
                var isSuccess = await _authorRepository.Create(record);
                if (!isSuccess)
                {
                    _logger.LogWarn("Create in db goes wrong");
                    return StatusCode(500, "Internal Error");
                }
                return Created("Created", new { record });
            }
            catch (Exception e)
            {

                _logger.LogInfo($"{e.Message} - {e.InnerException}");
                return StatusCode(500, "Internal Error");
            }
        }
        /// <summary>
        /// Update  entity of Author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthorDTO updateAuthorDTO)
        {
            try
            {
                if (id < 1 || updateAuthorDTO == null || id != updateAuthorDTO.Id)
                {
                    _logger.LogWarn("Bed request from Update0");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Bed request from Create1");
                    return BadRequest(ModelState);
                }
                var record = _mapper.Map<Author>(updateAuthorDTO);
                var isSuccess = await _authorRepository.Update(record);
                if (!isSuccess)
                {
                    _logger.LogWarn("Create in db goes wrong");
                    return StatusCode(500, "Internal Error");
                }
                return NoContent();
            }
            catch (Exception e)
            {

                _logger.LogInfo($"{e.Message} - {e.InnerException}");
                return StatusCode(500, "Internal Error");
            }
        }
        /// <summary>
        ///Delete  entity of Author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id < 1)
                {
                    _logger.LogWarn("Bed request from delete0");
                    return BadRequest(ModelState);
                }
               
                
                var record = await _authorRepository.FindById(id);
                if (record == null)
                {
                    _logger.LogWarn("Delete in db goes wrong");
                    return NotFound();
                }
                var isSuccess =await _authorRepository.Delete(record);
                if (!isSuccess)
                {
                    return StatusCode(500, "Internal Error");
                }
                return NoContent();
            }
            catch (Exception e)
            {

                _logger.LogInfo($"{e.Message} - {e.InnerException}");
                return StatusCode(500, "Internal Error");
            }
        }


    }
}
