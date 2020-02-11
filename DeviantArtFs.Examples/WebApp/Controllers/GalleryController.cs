﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviantArtFs.Examples.WebApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace DeviantArtFs.Examples.WebApp.Controllers
{
    public class GalleryController : ControllerBase
    {
        public GalleryController(ExampleDbContext context, DeviantArtAuth appReg) : base(context, appReg) { }

        public async Task<IActionResult> Index(string username = null, Guid? folderId = null, int offset = 0, int? limit = null)
        {
            var token = await GetAccessTokenAsync();
            if (token == null) return Forbid();

            var paging = new DeviantArtPagingParams { Offset = offset, Limit = limit };
            DeviantArtPagedResult<Deviation> resp;
            if (folderId is Guid f) {
                var r = await Requests.Gallery.GalleryById.ExecuteAsync(
                    token,
                    paging,
                    new Requests.Gallery.GalleryByIdRequest(f) { Username = username });
                resp = new DeviantArtPagedResult<Deviation>(r.has_more, r.next_offset, r.results);
            } else {
                resp = await Requests.Gallery.GalleryAllView.ExecuteAsync(
                    token,
                    paging,
                    new Requests.Gallery.GalleryAllViewRequest { Username = username });
            }

            ViewBag.Username = username;
            ViewBag.FolderId = folderId;
            ViewBag.Limit = limit;
            return View(resp);
        }

        public async Task<IActionResult> List(string username = null)
        {
            var token = await GetAccessTokenAsync();
            if (token == null) return Forbid();

            var list = await Requests.Gallery.GalleryFolders.ToArrayAsync(token, 0, 100, new Requests.Gallery.GalleryFoldersRequest { CalculateSize = true, Username = username });

            ViewBag.Username = username;
            return View(list);
        }
    }
}