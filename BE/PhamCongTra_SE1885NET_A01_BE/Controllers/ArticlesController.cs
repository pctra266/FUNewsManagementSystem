using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace FuNewsManagementAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)] 
    public class ArticlesController : ODataController
    {
        private readonly INewsArticleRepository _articleRepo;

        public ArticlesController(INewsArticleRepository articleRepo)
        {
            _articleRepo = articleRepo;
        }

        // GET odata/Articles
        [EnableQuery]
        [HttpGet]
        public IActionResult Get()
        {
            var result = _articleRepo.GetNewsArticles();
            return Ok(result);
        }

        // GET odata/Articles('A001')
        [EnableQuery]
        [HttpGet("({key})")]
        public IActionResult Get([FromRoute] string key)
        {
            var article = _articleRepo.GetNewsArticleById(key);
            if (article == null)
                return NotFound();

            return Ok(article);
        }
    }
}
