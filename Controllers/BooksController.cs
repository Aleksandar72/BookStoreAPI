using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStoreAPI.Contract;
using BookStoreAPI.Data;
using BookStoreAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreAPI.Controllers
{
    /// <summary>
    /// Endpoint for book store's API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IloggerService _logger;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        public BooksController(IBookRepository bookRepository, IAuthorRepository authorRepository, IloggerService logger, IMapper mapper, IWebHostEnvironment env)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
            _env = env;
        }
        /// <summary>
        /// Get all books without filtering
        /// </summary>
        /// <returns>List of authors in the system</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            try
            {
                _logger.LogInfo($"{GetCallLocation()}: Try to get all books");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                foreach (var book in response)
                {
                    if (!string.IsNullOrEmpty(book.Image))
                    {
                        if (System.IO.File.Exists(GetImgPath(book.Image)))
                        {
                            byte[]  imgBytes = System.IO.File.ReadAllBytes(GetImgPath(book.Image));
                            book.File = Convert.ToBase64String(imgBytes);
                        }
                    }
                }
                _logger.LogInfo($"{GetCallLocation()}:Success to get all books");
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogInfo($"{ GetCallLocation()}: { e.Message} - {e.InnerException}");
                return StatusCode(500, "Internal Error");
            }


        }
        /// <summary>
        /// Get book
        /// </summary>
        /// <returns>List of authors in the system</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            try
            {
                _logger.LogInfo($"{GetCallLocation()}: Try to get book");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogInfo($"{GetCallLocation()}: Don't exist");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                if (!string.IsNullOrEmpty(response.Image))
                {
                    var imgPath = GetImgPath(book.Image);
                    if (System.IO.File.Exists(imgPath))
                    {
                        byte[] imgBytes = System.IO.File.ReadAllBytes(imgPath);
                        response.File = Convert.ToBase64String(imgBytes);
                    }
                } 
                _logger.LogInfo($"{GetCallLocation()}:Success to book");
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogInfo($"{ GetCallLocation()}: { e.Message} - {e.InnerException}");
                return StatusCode(500, "Internal Error");
            }


        }
        /// <summary>
        /// Create new entity of Book
        /// </summary>
        /// <param name="Book"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateBookDTO createBookDTO)
        {
            try
            {
                if (createBookDTO == null)
                {
                    _logger.LogWarn($"{ GetCallLocation()}:Bed request from Create0");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{ GetCallLocation()}:Bed request from Create1");
                    return BadRequest(ModelState);
                }
                var record = _mapper.Map<Book>(createBookDTO);
                var isSuccess = await _bookRepository.Create(record);
                if (!isSuccess)
                {
                    _logger.LogWarn($"{ GetCallLocation()}:Create in db goes wrong");
                    return StatusCode(500, "Internal Error");
                }
                if (!string.IsNullOrEmpty(createBookDTO.File))
                {
                    var imgPath = $"{_env.ContentRootPath}\\uploads\\{createBookDTO.Image}";
                    byte[] imageBytes = Convert.FromBase64String(createBookDTO.File);
                    System.IO.File.WriteAllBytes(imgPath,imageBytes);
                }
                return Created("Created", new { record });
            }
            catch (Exception e)
            {
                _logger.LogInfo($"{ GetCallLocation()}:{e.Message} - {e.InnerException}");
                return StatusCode(500, "Internal Error");
            }
        }
        /// <summary>
        /// Update  entity of Book
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDTO updateBookDTO)
        {
            try
            {
                if (id < 1 || updateBookDTO == null || id != updateBookDTO.Id)
                {
                    _logger.LogWarn("Bed request from Update0");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Bed request from Create1");
                    return BadRequest(ModelState);
                }
                var oldImage = await _bookRepository.GetImageFileName(id);
                var record = _mapper.Map<Book>(updateBookDTO);
                var isSuccess = await _bookRepository.Update(record);
                if (!isSuccess)
                {
                    _logger.LogWarn("Create in db goes wrong");
                    return StatusCode(500, "Internal Error");
                }
                if (!updateBookDTO.Image.Equals(oldImage))
                {
                    if (System.IO.File.Exists(GetImgPath(oldImage)))
                    {
                        System.IO.File.Delete(GetImgPath(oldImage));
                    }

                }
                if (!string.IsNullOrEmpty(updateBookDTO.Image))
                {
                    byte[] imageBytes = Convert.FromBase64String(updateBookDTO.File);
                    System.IO.File.WriteAllBytes(GetImgPath(updateBookDTO.Image), imageBytes);
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
        ///Delete  entity of Book
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
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
                var record = await _bookRepository.FindById(id);
                if (record == null)
                {
                    _logger.LogWarn("Delete in db goes wrong");
                    return NotFound();
                }
                var isSuccess = await _bookRepository.Delete(record);
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
        private string GetCallLocation()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }
        private string GetImgPath(string fileName) => $"{_env.ContentRootPath}\\uploads\\{fileName}";
       
    }
}
