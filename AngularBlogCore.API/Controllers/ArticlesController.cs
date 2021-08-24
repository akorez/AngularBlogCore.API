using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AngularBlogCore.API.Models;
using AngularBlogCore.API.Responses;
using System.Globalization;

namespace AngularBlogCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly AngularBlogDBContext _context;

        public ArticlesController(AngularBlogDBContext context)
        {
            _context = context;
        }

        // GET: api/Articles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles()
        {
            return await _context.Articles.ToListAsync();
        }

        [HttpGet("{page}/{pageSize}")]
        public IActionResult GetArticles(int page = 1, int pageSize = 5)
        {
            System.Threading.Thread.Sleep(3000);

            try
            {
                IQueryable<Article> query;

                query = _context.Articles.Include(x => x.Category).Include(y => y.Comments).OrderByDescending(z => z.PublishDate);

                int totalCount = query.Count();

                var articleResponse = query.Skip(pageSize * (page - 1)).Take(5).ToList().Select(x => new ArticleResponse()
                {
                    Id = x.Id,
                    Title = x.Title,
                    ContentMain = x.ContentMain,
                    ContentSummary = x.ContentSummary,
                    Picture = x.Picture,
                    PublishDate = x.PublishDate,
                    CommentCount = x.Comments.Count(),
                    ViewCount = x.ViewCount,
                    Category = new CategoryResponse() { Id = x.Category.Id, Name = x.Category.Name }

                });

                var result = new
                {
                    TotalCount = totalCount,
                    Articles = articleResponse
                };

                return Ok(result);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        [Route("GetArticlesWithCategory/{categoryId}/{page}/{pageSize}")]
        [HttpGet]
        public IActionResult GetArticlesWithCategory(int categoryId, int page = 1, int pageSize = 5)
        {
            System.Threading.Thread.Sleep(2500);
            try
            {
                IQueryable<Article> query = _context.Articles.Include(x => x.Category).Include(y => y.Comments).Where(z => z.CategoryId == categoryId)
                        .OrderByDescending(x => x.PublishDate);

                var queryResult = ArticlePagination(query, page, pageSize);

                var result = new
                {
                    TotalCount = queryResult.Item2,
                    Articles = queryResult.Item1
                };

                return Ok(result);

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        [Route("GetSearchArticle/{searchText}/{page}/{pageSize}")]
        [HttpGet]
        public IActionResult SearchArticle(string searchText,int page = 1, int pageSize = 5)
        {
            IQueryable<Article> query;
            query = _context.Articles.Include(x=>x.Category).Include(y=>y.Comments).Where(z=>z.Title.Contains(searchText))
                .OrderByDescending(x => x.PublishDate);

            var queryResult = ArticlePagination(query, page, pageSize);

            var result = new
            {
                TotalCount = queryResult.Item2,
                Articles = queryResult.Item1
            };

            return Ok(result);
        }

        [Route("GetArticlesByMostView")]
        [HttpGet]        
        public IActionResult GetArticlesByMostView()
        {
            System.Threading.Thread.Sleep(2000);

            var articles = _context.Articles.OrderByDescending(x => x.ViewCount).Take(5).Select(x => new ArticleResponse()
            {
                Id = x.Id,
                Title = x.Title
            });

            return Ok(articles);
        }
        
        [Route("GetArticlesArchive")]
        [HttpGet]        
        public IActionResult GetArticlesArchive()
        {
            System.Threading.Thread.Sleep(1000);

            var query = _context.Articles.GroupBy(x => new { x.PublishDate.Year, x.PublishDate.Month }).Select(y =>
                new
                {
                    year = y.Key.Year,
                    month = y.Key.Month,
                    count = y.Count(),
                    monthName = new DateTime(y.Key.Year,y.Key.Month,1).ToString("MMMM")
                }); 

            return Ok(query);
        }

        [Route("GetArticleArchiveList/{year}/{month}/{page}/{pageSize}")]
        [HttpGet]
        public IActionResult GetArticleArchiveList(int year,int month,int page,int pageSize)
        {
            System.Threading.Thread.Sleep(1700);

            IQueryable<Article> query;
            query = _context.Articles.Include(x => x.Category).Include(y => y.Comments).Where(z => z.PublishDate.Year == year && z.PublishDate.Month == month)
                .OrderByDescending(x => x.PublishDate);

            var queryResult = ArticlePagination(query, page, pageSize);

            var result = new
            {
                TotalCount = queryResult.Item2,
                Articles = queryResult.Item1
            };

            return Ok(result);
        }
        // GET: api/Articles/5
        [HttpGet("{id}")]
        public IActionResult GetArticle(int id)
        {
            System.Threading.Thread.Sleep(2500);

            var article = _context.Articles.Include(x => x.Category).Include(y => y.Comments).FirstOrDefault(z => z.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            ArticleResponse articleResponse = new ArticleResponse()
            {
                Id = article.Id,
                Title = article.Title,
                ContentMain = article.ContentMain,
                ContentSummary = article.ContentSummary,
                Picture = article.Picture,
                PublishDate = article.PublishDate,
                ViewCount = article.ViewCount,
                Category = new CategoryResponse() { Id = article.Category.Id, Name = article.Category.Name },
                CommentCount = article.Comments.Count
            };

            return Ok(articleResponse);
        }

        // PUT: api/Articles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, Article article)
        {
            if (id != article.Id)
            {
                return BadRequest();
            }

            _context.Entry(article).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Articles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticle", new { id = article.Id }, article);
        }

        // DELETE: api/Articles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }

        internal System.Tuple<IEnumerable<ArticleResponse>, int> ArticlePagination(IQueryable<Article> query, int page, int pageSize)
        {
            int totalCount = query.Count();

            var articleResponse = query.Skip(pageSize * (page - 1)).Take(pageSize).ToList().Select(x => new ArticleResponse()
            {
                Id = x.Id,
                Title = x.Title,
                ContentMain = x.ContentMain,
                ContentSummary = x.ContentSummary,
                Picture = x.Picture,
                PublishDate = x.PublishDate,
                CommentCount = x.Comments.Count(),
                ViewCount = x.ViewCount,
                Category = new CategoryResponse() { Id = x.Category.Id, Name = x.Category.Name }

            });

            return new System.Tuple<IEnumerable<ArticleResponse>, int>(articleResponse, totalCount);
        }

        [Route("ArticleViewCountUp/{id}")]
        [HttpGet]
        public IActionResult ArticleViewCountUp (int id)
        {
            var article = _context.Articles.Find(id);

            article.ViewCount += 1;

            _context.SaveChanges();

            return Ok();
        }
    }
}
