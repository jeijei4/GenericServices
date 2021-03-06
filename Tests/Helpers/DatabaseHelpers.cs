﻿#region licence
// The MIT License (MIT)
// 
// Filename: DatabaseHelpers.cs
// Date Created: 2014/06/11
// 
// Copyright (c) 2014 Jon Smith (www.selectiveanalytics.com & www.thereformedprogrammer.net)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GenericServices;
using GenericServices.Services.Concrete;
using GenericServices.ServicesAsync.Concrete;
using Tests.DataClasses;
using Tests.DataClasses.Concrete;
using Tests.DTOs.Concrete;

namespace Tests.Helpers
{
    static class DatabaseHelpers
    {
        #region EF methods

        //Various db methods using the best hand-coded EF methods

        public static void ListEfDirect<T>(this SampleWebAppDb db, int id) where T : class
        {
            var num = db.Set<T>().AsNoTracking().ToList().Count;
        }

        public static async Task ListEfDirectAsync<T>(this SampleWebAppDb db, int id) where T : class
        {
            var num = (await db.Set<T>().AsNoTracking().ToListAsync()).Count;
        }

        public static void ListPostEfViaDto(this SampleWebAppDb db, int id)
        {
            var num = db.Set<Post>().AsNoTracking().Select(x => new SimplePostDto
            {
                PostId = x.PostId,
                BloggerName = x.Blogger.Name,
                Title = x.Title,
                Tags = x.Tags,
                LastUpdated = x.LastUpdated
            }).ToList().Count;
        }

        public static async Task ListPostEfViaDtoAsync(this SampleWebAppDb db, int id)
        {
            var list = await db.Set<Post>().AsNoTracking().Select(x => new SimplePostDto
            {
                PostId = x.PostId,
                BloggerName = x.Blogger.Name,
                Title = x.Title,
                Tags = x.Tags,
                LastUpdated = x.LastUpdated
            }).ToListAsync();
        }

        //-------------------

        public static void CreatePostEfDirect(this SampleWebAppDb db, int id)
        {
            var guidString = Guid.NewGuid().ToString("N");
            var postClass = new Post
            {
                Title = guidString,
                Content = guidString,
                Blogger = db.Blogs.First(),
                Tags = new Collection<Tag> {  db.Tags.First()}
            };
            db.Posts.Add(postClass);
            var status = db.SaveChangesWithChecking();
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task CreatePostEfDirectAsync(this SampleWebAppDb db, int id)
        {
            var guidString = Guid.NewGuid().ToString("N");
            var postClass = new Post
            {
                Title = guidString,
                Content = guidString,
                Blogger = db.Blogs.First(),
                Tags = new Collection<Tag> { db.Tags.First() }
            };
            db.Posts.Add(postClass);
            var status = await db.SaveChangesWithCheckingAsync();
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        //-----------------------------------------

        public static void UpdatePostEfDirect(this SampleWebAppDb db, int postId)
        {

            var postClass = db.Posts.Include( x => x.Blogger).Include( x => x.Tags).Single( x => x.PostId == postId);

            var guidString = Guid.NewGuid().ToString("N");
            postClass.Title = guidString;
            postClass.Content = guidString;
            postClass.Blogger = db.Blogs.First();
            postClass.Tags = new Collection<Tag> {db.Tags.First()};

            var status = db.SaveChangesWithChecking();
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task UpdatePostEfDirectAsync(this SampleWebAppDb db, int postId)
        {

            var postClass = await db.Posts.Include(x => x.Blogger).Include(x => x.Tags).SingleAsync(x => x.PostId == postId);

            var guidString = Guid.NewGuid().ToString("N");
            postClass.Title = guidString;
            postClass.Content = guidString;
            postClass.Blogger = db.Blogs.First();
            postClass.Tags = new Collection<Tag> { db.Tags.First() };

            var status = await db.SaveChangesWithCheckingAsync();
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        //---------------------------------------

        public static void DeletePostEfDirect(this SampleWebAppDb db, int postId)
        {
            var postClass = new Post
            {
                PostId = postId
            };
            db.Entry(postClass).State = EntityState.Deleted;
            var status = db.SaveChangesWithChecking();
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task DeletePostEfDirectAsync(this SampleWebAppDb db, int postId)
        {
            var postClass = new Post
            {
                PostId = postId
            };
            db.Entry(postClass).State = EntityState.Deleted;
            var status = await db.SaveChangesWithCheckingAsync();
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        #endregion

        #region Generic versions
        //=====================================================
        //now the self-selecting

        public static void ListGenericDirect<T>(this SampleWebAppDb db, int id) where T : class
        {
            var service = new ListService<T>(db);
            var num = service.GetAll().ToList().Count;
        }

        public static async Task ListGenericDirectAsync<T>(this SampleWebAppDb db, int id) where T : class
        {
            var service = new ListService<T>(db);
            var list = await service.GetAll().ToListAsync();
        }

        public static void ListPostGenericViaDto(this SampleWebAppDb db, int id)
        {
            var service = new ListService<Post,SimplePostDto>(db);
            var num = service.GetAll().ToList().Count;
        }

        public static async Task ListPostGenericViaDtoAsync(this SampleWebAppDb db, int id)
        {
            var service = new ListService<Post, SimplePostDto>(db);
            var list = await service.GetAll().ToListAsync();
        }

        //--------

        public static void CreatePostGenericDirect(this SampleWebAppDb db, int id)
        {
            
            var guidString = Guid.NewGuid().ToString("N");
            var postClass = new Post
            {
                Title = guidString,
                Content = guidString,
                Blogger = db.Blogs.First(),
                Tags = new Collection<Tag> { db.Tags.First() }
            };
            
            var service = new CreateService<Post>(db);
            var status = service.Create(postClass);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task CreatePostGenericDirectAsync(this SampleWebAppDb db, int id)
        {

            var guidString = Guid.NewGuid().ToString("N");
            var postClass = new Post
            {
                Title = guidString,
                Content = guidString,
                Blogger = db.Blogs.First(),
                Tags = new Collection<Tag> { db.Tags.First() }
            };

            var service = new CreateServiceAsync<Post>(db);
            var status = await service.CreateAsync(postClass);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        //-----------------

        public static void UpdatePostGenericDirect(this SampleWebAppDb db, int postId)
        {
            var setupService = new DetailService<Post>(db);
            var setupStatus = setupService.GetDetailUsingWhere(x => x.PostId == postId);
            setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);

            var guidString = Guid.NewGuid().ToString("N");
            setupStatus.Result.Title = guidString;
            setupStatus.Result.Content = guidString;
            setupStatus.Result.Tags = new Collection<Tag> { db.Tags.First() };

            var service = new UpdateService<Post>(db);
            var status = service.Update(setupStatus.Result);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task UpdatePostGenericDirectAsync(this SampleWebAppDb db, int postId)
        {
            var setupService = new DetailServiceAsync<Post>(db);
            var setupStatus = await setupService.GetDetailAsync(postId);
            setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);

            var guidString = Guid.NewGuid().ToString("N");
            setupStatus.Result.Title = guidString;
            setupStatus.Result.Content = guidString;
            setupStatus.Result.Tags = new Collection<Tag> { db.Tags.First() };

            var service = new UpdateServiceAsync<Post>(db);
            var status = await service.UpdateAsync(setupStatus.Result);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static void UpdatePostGenericViaDto(this SampleWebAppDb db, int postId)
        {
            var setupService = new UpdateSetupService<Post,DetailPostDto>(db);
            var setupStatus = setupService.GetOriginal(postId);
            setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);

            var guidString = Guid.NewGuid().ToString("N");
            setupStatus.Result.Title = guidString;
            setupStatus.Result.Content = guidString;
            setupStatus.Result.Tags = new Collection<Tag> { db.Tags.First() }; 

            var service = new UpdateService<Post,DetailPostDto>(db);
            var status = service.Update(setupStatus.Result);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task UpdatePostGenericViaDtoAsync(this SampleWebAppDb db, int postId)
        {
            var setupService = new UpdateSetupServiceAsync<Post, DetailPostDtoAsync>(db);
            var setupStatus = await setupService.GetOriginalAsync(postId);
            setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);

            var guidString = Guid.NewGuid().ToString("N");
            setupStatus.Result.Title = guidString;
            setupStatus.Result.Content = guidString;
            setupStatus.Result.Tags = new Collection<Tag> { db.Tags.First() }; 

            var service = new UpdateServiceAsync<Post, DetailPostDtoAsync>(db);
            var status = await service.UpdateAsync(setupStatus.Result);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        //--------------------------

        public static void DeletePostGenericDirect(this SampleWebAppDb db, int postId)
        {
            var service = new DeleteService(db);
            var status = service.Delete<Post>(postId);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task DeletePostGenericDirectAsync(this SampleWebAppDb db, int postId)
        {
            var service = new DeleteServiceAsync(db);
            var status = await service.DeleteAsync<Post>(postId);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        #endregion

        #region GSelect services, self selecting
        //=====================================================
        //now the generic versions

        public static void ListGSelectDirect<T>(this SampleWebAppDb db, int id) where T : class, new()
        {
            var service = new ListService(db);
            var num = service.GetAll<T>().ToList().Count;
        }

        public static async Task ListGSelectDirectAsync<T>(this SampleWebAppDb db, int id) where T : class, new()
        {
            var service = new ListService(db);
            var list = await service.GetAll<T>().ToListAsync();
        }

        public static void ListPostGSelectViaDto(this SampleWebAppDb db, int id)
        {
            var service = new ListService(db);
            var num = service.GetAll<SimplePostDto>().ToList().Count;
        }

        public static async Task ListPostGSelectViaDtoAsync(this SampleWebAppDb db, int id)
        {
            var service = new ListService(db);
            var list = await service.GetAll<SimplePostDto>().ToListAsync();
        }

        //--------

        public static void CreatePostGSelectDirect(this SampleWebAppDb db, int id)
        {

            var guidString = Guid.NewGuid().ToString("N");
            var postClass = new Post
            {
                Title = guidString,
                Content = guidString,
                Blogger = db.Blogs.First(),
                Tags = new Collection<Tag> {db.Tags.First()}
            };

            var service = new CreateService(db);
            var status = service.Create(postClass);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task CreatePostGSelectDirectAsync(this SampleWebAppDb db, int id)
        {

            var guidString = Guid.NewGuid().ToString("N");
            var postClass = new Post
            {
                Title = guidString,
                Content = guidString,
                Blogger = db.Blogs.First(),
                Tags = new Collection<Tag> { db.Tags.First() }
            };

            var service = new CreateServiceAsync(db);
            var status = await service.CreateAsync(postClass);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        //-----------------

        public static void UpdatePostGSelectDirect(this SampleWebAppDb db, int postId)
        {
            var setupService = new DetailService(db);
            var setupStatus = setupService.GetDetail<Post>(postId);
            setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);

            var guidString = Guid.NewGuid().ToString("N");
            setupStatus.Result.Title = guidString;
            setupStatus.Result.Content = guidString;
            setupStatus.Result.Tags = new Collection<Tag> { db.Tags.First() };

            var service = new UpdateService(db);
            var status = service.Update(setupStatus.Result);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task UpdatePostGSelectDirectAsync(this SampleWebAppDb db, int postId)
        {
            var setupService = new DetailServiceAsync(db);
            var setupStatus = await setupService.GetDetailAsync<Post>(postId);
            setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);

            var guidString = Guid.NewGuid().ToString("N");
            setupStatus.Result.Title = guidString;
            setupStatus.Result.Content = guidString;
            setupStatus.Result.Tags = new Collection<Tag> { db.Tags.First() };

            var service = new UpdateServiceAsync<Post>(db);
            var status = await service.UpdateAsync(setupStatus.Result);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static void UpdatePostGSelectViaDto(this SampleWebAppDb db, int postId)
        {
            var setupService = new UpdateSetupService(db);
            var setupStatus = setupService.GetOriginal<DetailPostDto>(postId);
            setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);

            var guidString = Guid.NewGuid().ToString("N");
            setupStatus.Result.Title = guidString;
            setupStatus.Result.Content = guidString;
            setupStatus.Result.Tags = new Collection<Tag> { db.Tags.First() };

            var service = new UpdateService(db);
            var status = service.Update(setupStatus.Result);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        public static async Task UpdatePostGSelectViaDtoAsync(this SampleWebAppDb db, int postId)
        {
            var setupService = new UpdateSetupServiceAsync(db);
            var setupStatus = await setupService.GetOriginalAsync<DetailPostDtoAsync>(postId);
            setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);

            var guidString = Guid.NewGuid().ToString("N");
            setupStatus.Result.Title = guidString;
            setupStatus.Result.Content = guidString;
            setupStatus.Result.Tags = new Collection<Tag> { db.Tags.First() };

            var service = new UpdateServiceAsync(db);
            var status = await service.UpdateAsync(setupStatus.Result);
            status.IsValid.ShouldEqual(true, status.Errors);
        }

        #endregion

        //-------------------------------------------

        public static void FillComputedNAll(this SampleWebAppDb db, int totalOfEach)
        {
            //clear the current
            ClearDatabase(db);

            var tags = BuildTags(totalOfEach);
            db.Tags.AddRange(tags);
            var bloggers = BuildBloggers(totalOfEach);
            db.Blogs.AddRange(bloggers);
            var posts = BuildPostsNAll(tags, bloggers, totalOfEach);
            db.Posts.AddRange(posts);
            var grades = BuildGrades(tags, posts, totalOfEach);
            db.PostTagGrades.AddRange(grades);
            var status = db.SaveChangesWithChecking();
            status.IsValid.ShouldEqual(true, status.Errors);
        }


        public static void FillComputedNPost(this SampleWebAppDb db, int totalOfEach)
        {
            //clear the current
            ClearDatabase(db);

            var tag1 = new Tag {Name = "Tag1", Slug = "tag1"};
            var tag2 = new Tag { Name = "Tag2", Slug = "tag2" };

            db.Tags.AddRange(new[] { tag1, tag2 });
            var blogger1 = new Blog() {Name = "Name1", EmailAddress = "Email1"};
            var blogger2 = new Blog() { Name = "Name2", EmailAddress = "Email2" };

            db.Blogs.AddRange(new[] { blogger1, blogger2 });
            var posts = BuildNPosts(tag2, blogger2, totalOfEach);
            db.Posts.AddRange(posts);

            var status = db.SaveChangesWithChecking();
            status.IsValid.ShouldEqual(true, status.Errors);
        }


        private static List<Tag> BuildTags(int totalOfEach)
        {
            var result = new List<Tag>();
            for (int i = 0; i < totalOfEach; i++)
            {
                var guidString = Guid.NewGuid().ToString("N");
                result.Add(new Tag
                {
                    Name = guidString,
                    Slug = guidString
                });
            }
            return result;
        }

        private static List<Blog> BuildBloggers(int totalOfEach)
        {
            var result = new List<Blog>();
            for (int i = 0; i < totalOfEach; i++)
            {
                var guidString = Guid.NewGuid().ToString("N");
                result.Add(new Blog
                {
                    Name = guidString,
                    EmailAddress = guidString
                });
            }
            return result;
        }

        private static List<Post> BuildPostsNAll(List<Tag> tags, List<Blog> bloggers, int totalOfEach)
        {
            var result = new List<Post>();
            for (int i = 0; i < totalOfEach; i++)
            {
                var guidString = Guid.NewGuid().ToString("N");
                result.Add(new Post
                {
                    Title = guidString,
                    Content = guidString,
                    Blogger = bloggers[i],
                    Tags = new Collection<Tag> { tags[i / 2], tags[(totalOfEach - 1 + i) / 2] }
                });
            }
            return result;
        }

        private static List<Post> BuildNPosts(Tag tag, Blog blogger, int totalOfEach)
        {
            var result = new List<Post>();
            for (int i = 0; i < totalOfEach; i++)
            {
                var guidString = Guid.NewGuid().ToString("N");
                result.Add(new Post
                {
                    Title = guidString,
                    Content = guidString,
                    Blogger = blogger,
                    Tags = new Collection<Tag> { tag }
                });
            }
            return result;
        }

        private static List<PostTagGrade> BuildGrades(List<Tag> tags, List<Post> posts, int totalOfEach)
        {
            var result = new List<PostTagGrade>();
            for (int i = 0; i < totalOfEach; i++)
            {
                result.Add(new PostTagGrade
                {
                    PostPart = posts[i],
                    TagPart = tags[i],
                    Grade = 1
                });
            }
            return result;
        }


        private static void ClearDatabase(SampleWebAppDb db)
        {
            db.Posts.RemoveRange(db.Posts);
            db.Tags.RemoveRange(db.Tags);
            db.Blogs.RemoveRange(db.Blogs);
            db.PostTagGrades.RemoveRange(db.PostTagGrades);
            db.SaveChanges();
        }

    }
}
