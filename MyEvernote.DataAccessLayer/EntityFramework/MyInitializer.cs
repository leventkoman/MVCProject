using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using MyEvernote.Entities;

namespace MyEvernote.DataAccessLayer.EntityFramework
{
   public class MyInitializer:CreateDatabaseIfNotExists<DatabaseContext>
    {
        protected override void Seed(DatabaseContext context)
        {
            EvernoteUser admin = new EvernoteUser()
            {
                Name = "Levent",
                Surname = "KOMAN",
                Email = "levent2877@gmail.com",
                ActivateGuid = Guid.NewGuid(),
                IsActive = true,
                IsAdmin = true,
                Username = "leventkoman",
                Password = "1234",
                ProfileImageFileName = "images.jpg",
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now.AddMinutes(5),
                ModifiedUsername = "leventkoman"
            };
            EvernoteUser standartUser = new EvernoteUser()
            {
                Name = "Mervenur",
                Surname = "KOMAN",
                Email = "mervenurkoman@gmail.com",
                ActivateGuid = Guid.NewGuid(),
                IsActive = true,
                IsAdmin = false,
                Username = "mervenurkoman",
                Password = "123456",
                ProfileImageFileName = "images.jpg",
                CreatedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                ModifiedOn = DateTime.Now.AddMinutes(65),
                ModifiedUsername = "mervenurkoman"
            };
            context.EverNoteUsers.Add(admin);
            context.EverNoteUsers.Add(standartUser);
            for (int i = 0; i < 8; i++)
            {
                EvernoteUser user = new EvernoteUser()
                {
                    Name = FakeData.NameData.GetFirstName(),
                    Surname = FakeData.NameData.GetSurname(),
                    Email = FakeData.NetworkData.GetEmail(),
                    ProfileImageFileName = "images.jpg",
                    ActivateGuid = Guid.NewGuid(),
                    IsActive = true,
                    IsAdmin = false,
                    Username = $"user{i}",
                    Password = "123",
                    CreatedOn = DateTime.Now.AddHours(1),
                    ModifiedOn = DateTime.Now.AddMinutes(65),
                    ModifiedUsername = $"user{i}"
                };
                context.EverNoteUsers.Add(user);
            }
            context.SaveChanges();

            //fake categories
            List<EvernoteUser> userlist = context.EverNoteUsers.ToList();

            for (int i = 0; i < 10; i++)
            {
                Category cat = new Category()
                {
                    Title = FakeData.PlaceData.GetStreetName(),
                    Description = FakeData.PlaceData.GetAddress(),
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    ModifiedUsername = "leventkoman"
                };
                context.Categories.Add(cat);

                //Adding fake notes
                for (int j = 0; j < FakeData.NumberData.GetNumber(5,9); j++)
                {

                    EvernoteUser owner = userlist[FakeData.NumberData.GetNumber(0, userlist.Count - 1)];

                    Note note = new Note()
                    {
                        Title = FakeData.TextData.GetAlphaNumeric(FakeData.NumberData.GetNumber(5, 25)),
                        Text = FakeData.TextData.GetSentences(FakeData.NumberData.GetNumber(5, 25)),
                        Category = cat,
                        IsDraft = false,
                        LikeCount = FakeData.NumberData.GetNumber(1, 9),
                        Owner = userlist[FakeData.NumberData.GetNumber(0, userlist.Count - 1)],
                        CreatedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                        ModifiedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                        ModifiedUsername = owner.Username
                    };

                    cat.Notes.Add(note);
                    //Adding comments
                    for (int k = 0; k < FakeData.NumberData.GetNumber(3,5); k++)
                    {
                        EvernoteUser comment_owner = userlist[FakeData.NumberData.GetNumber(0, userlist.Count - 1)];

                        Comment comment = new Comment()
                        {
                            Text = FakeData.TextData.GetSentence(),
                            Note = note,
                            Owner = userlist[FakeData.NumberData.GetNumber(0, userlist.Count - 1)],
                            CreatedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                            ModifiedOn = FakeData.DateTimeData.GetDatetime(DateTime.Now.AddYears(-1), DateTime.Now),
                            ModifiedUsername = comment_owner.Username
                        };
                        note.Comments.Add(comment);
                    }
                    //adding fake likes

                    for (int l = 0; l < note.LikeCount; l++)
                    {
                        Liked like=new Liked()
                        {
                            LikedUser = userlist[l]
                        };
                        note.Likes.Add(like);
                    }
                }
            }

            context.SaveChanges();
        }
    }
}
