﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketCore.Data.Knowledgebase.Queries;
using TicketCore.Web.Filters;

namespace TicketCore.Web.Areas.User.Controllers
{
    [SessionTimeOut]
    [AuthorizeAgent]
    [Area("User")]
    [ServiceFilter(typeof(AuditFilterAttribute))]
    public class SearchController : Controller
    {
        private readonly IKnowledgebaseQueries _iKnowledgebaseQueries;
        public SearchController(IKnowledgebaseQueries knowledgebaseQueries)
        {
            _iKnowledgebaseQueries = knowledgebaseQueries;
        }
        public IActionResult Article(string departmentId)
        {
            var result = _iKnowledgebaseQueries.SearchKnowledgebasebydepartmentId(departmentId);
            return View(result);
        }

        public ActionResult GetAllArticle(string searchtext)
        {
            try
            {
                return Json(_iKnowledgebaseQueries.SearchKnowledgebase(searchtext));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public IActionResult ArticleView(int id)
        {
            try
            {
                var knowledgebaseViewModel = _iKnowledgebaseQueries.GetKnowledgebaseDetailsForArticle(id);
                knowledgebaseViewModel.ListofAttachments = _iKnowledgebaseQueries.GetListAttachmentsByKnowledgebaseId(id);
                return View(knowledgebaseViewModel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult AllArticles()
        {
            try
            {
                var allKnowledgebase = _iKnowledgebaseQueries.AllKnowledgebase();
                return View(allKnowledgebase);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
