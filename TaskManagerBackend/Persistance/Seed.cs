using Domain;
using Domain.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance
{
    public static class Seed
    {
        public static async Task Initialize(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, DataContext context)
        {
            #region Roles

            await roleManager.CreateAsync(new IdentityRole("admin"));
            await roleManager.CreateAsync(new IdentityRole("user"));

            #endregion

            #region Users

            var adminUser = new AppUser
            {
                FirstName = "Aarshdeep",
                LastName = "Admin",
                UserName = "ascnyc29@gmail.com",
                Email = "ascnyc29@gmail.com"
            };

            var userUser = new AppUser
            {
                FirstName = "Aarshdeep",
                LastName = "Chadha",
                UserName = "aarshdeep.chadha@indianic.com",
                Email = "aarshdeep.chadha@indianic.com"
            };
            if (!userManager.Users.Any())
            {
                await userManager.CreateAsync(adminUser, "Pa$$w0rd");
                await userManager.CreateAsync(userUser, "Pa$$w0rd");

                await userManager.AddToRoleAsync(adminUser, "admin");
                await userManager.AddToRoleAsync(userUser, "user");
            }
            #endregion
            var categories = new List<TaskCategory>();
            #region Categories
            if (!context.TaskCategories.Any())
            {
                categories = new List<TaskCategory>
            {
                new TaskCategory
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Marketing",
                    CategoryDescription = "Tasks related to marketing activities.",
                    CreatedById = adminUser.Id
                },
                new TaskCategory
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Finance",
                    CategoryDescription = "Tasks related to financial management and analysis.",
                    CreatedById = adminUser.Id
                },
                new TaskCategory
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Human Resources",
                    CategoryDescription = "Tasks related to HR activities and employee management.",
                    CreatedById = adminUser.Id
                },
                new TaskCategory
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Product Development",
                    CategoryDescription = "Tasks related to the development and improvement of products.",
                    CreatedById = adminUser.Id
                },
                new TaskCategory
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Research",
                    CategoryDescription = "Tasks related to market research and analysis.",
                    CreatedById = adminUser.Id
                }
            };


                await context.TaskCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
            #endregion

            #region Tasks
            var tasks = new List<TaskEntity>();
            var adminId = adminUser.Id;
            var userId = userUser.Id;
            if (!context.TaskEntities.Any())
            {
                tasks = new List<TaskEntity>
                {
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Develop Marketing Strategy",
                        TaskPriority = TaskPriorityStatus.High,
                        Status = TaskStatusEnum.InProgress,
                        TaskDescription = "Create a comprehensive marketing strategy for the upcoming product launch.",
                        CategoryId = categories[2].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(14),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    },
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Review Financial Reports",
                        TaskPriority = TaskPriorityStatus.Low,
                        Status = TaskStatusEnum.NotStarted,
                        TaskDescription = "Analyze and review the financial reports for the last quarter.",
                        CategoryId = categories[3].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(7),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    },
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Organize Team Building Event",
                        TaskPriority = TaskPriorityStatus.Low,
                        Status = TaskStatusEnum.NotStarted,
                        TaskDescription = "Plan and organize a team building event for the department.",
                        CategoryId = categories[4].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(21),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    },
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Conduct Product Training",
                        TaskPriority = TaskPriorityStatus.High,
                        Status = TaskStatusEnum.InProgress,
                        TaskDescription = "Conduct training sessions for the sales team on the new product features.",
                        CategoryId = categories[1].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(10),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    },
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Write Quarterly Report",
                        TaskPriority = TaskPriorityStatus.Critical,
                        Status = TaskStatusEnum.NotStarted,
                        TaskDescription = "Compile and write the quarterly report summarizing departmental achievements and challenges.",
                        CategoryId = categories[3].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(14),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    },
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Update Website Content",
                        TaskPriority = TaskPriorityStatus.Low,
                        Status = TaskStatusEnum.InProgress,
                        TaskDescription = "Review and update the content on the company website to reflect latest offerings.",
                        CategoryId = categories[2].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(7),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    },
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Design Product Packaging",
                        TaskPriority = TaskPriorityStatus.High,
                        Status = TaskStatusEnum.NotStarted,
                        TaskDescription = "Work with the design team to create new packaging designs for the product line.",
                        CategoryId = categories[2].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(21),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    },
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Attend Industry Conference",
                        TaskPriority = TaskPriorityStatus.Critical,
                        Status = TaskStatusEnum.InProgress,
                        TaskDescription = "Register and attend the upcoming industry conference to network and gain insights.",
                        CategoryId = categories[1].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(10),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    },
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Develop Employee Training Program",
                        TaskPriority = TaskPriorityStatus.Low,
                        Status = TaskStatusEnum.NotStarted,
                        TaskDescription = "Create a comprehensive training program for new employees covering company policies and procedures.",
                        CategoryId = categories[4].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(14),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    },
                    new TaskEntity
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Research Market Trends",
                        TaskPriority = TaskPriorityStatus.High,
                        Status = TaskStatusEnum.NotStarted,
                        TaskDescription = "Conduct research on current market trends and competitor analysis to inform business strategy.",
                        CategoryId = categories[2].CategoryId,
                        DueDate = DateTime.UtcNow.AddDays(21),
                        AssignedBy = adminId,
                        CreatedById = adminId,
                    }
                    // Add more tasks as needed
                };

                await context.TaskEntities.AddRangeAsync(tasks);
                await context.SaveChangesAsync();
            }
            #endregion

            #region Comments
            if (!context.Comments.Any())
            {
                var comments = new List<Comment>
                {
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "Great progress on the marketing campaign! Let's keep up the momentum.",
                        CommentedById = adminId,
                        TaskId = tasks[0].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "Could you please provide more details on the revenue trends? It will help in the analysis.",
                        CommentedById = adminId,
                        TaskId = tasks[1].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "Looking forward to the team building event! Any suggestions for activities?",
                        CommentedById = adminId,
                        TaskId = tasks[2].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "The product training session was very informative. Thanks for organizing!",
                        CommentedById = adminId,
                        TaskId = tasks[3].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "I've started drafting the quarterly report. Will share the progress soon.",
                        CommentedById = adminId,
                        TaskId = tasks[4].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "The website content updates look good. Let's ensure all sections are covered.",
                        CommentedById = adminId,
                        TaskId = tasks[5].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "The packaging designs are coming along nicely. Do we have any customer feedback?",
                        CommentedById = adminId,
                        TaskId = tasks[6].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "I've registered for the industry conference. Excited to learn from industry experts.",
                        CommentedById = adminId,
                        TaskId = tasks[7].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "Let's include modules on company culture and values in the employee training program.",
                        CommentedById = adminId,
                        TaskId = tasks[8].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "The market research findings are insightful. We need to delve deeper into customer preferences.",
                        CommentedById = adminId,
                        TaskId = tasks[9].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "I've reviewed the marketing materials. They're ready for the next phase of testing.",
                        CommentedById = adminId,
                        TaskId = tasks[0].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "Can we schedule a budget review meeting next week to discuss the forecast updates?",
                        CommentedById = adminId,
                        TaskId = tasks[5].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "The employee benefits program draft is complete. Let's gather feedback from the team.",
                        CommentedById = adminId,
                        TaskId = tasks[6].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "The product testing results are promising. We should proceed with the next phase.",
                        CommentedById = adminId,
                        TaskId = tasks[7].TaskId,
                        CreatedById = adminId
                    },
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        CommentDescription = "The competitor analysis report is comprehensive. Let's highlight key findings in the presentation.",
                        CommentedById = adminId,
                        TaskId = tasks[8].TaskId,
                        CreatedById = adminId
                    }
                    // Add more comments as needed
                };


                await context.Comments.AddRangeAsync(comments);
                await context.SaveChangesAsync();
            }
            #endregion
        }
    }
}
