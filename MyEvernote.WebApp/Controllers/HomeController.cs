using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyEvernote.BusinessLayer;
using MyEvernote.Entities;
using MyEvernote.Entities.Messages;
using System.Net;
using System.Runtime.Remoting.Messaging;
using MyEvernote.Entities.ValueObjects;
using MyEvernote.WebApp.ViewModels;
using MyEvernote.BusinessLayer.Results;
using MyEvernote.WebApp.Models;
using MyEvernote.WebApp.Filters;

namespace MyEvernote.WebApp.Controllers
{
    [Exc]
    public class HomeController : Controller
    {
        NoteMenager noteMenager = new NoteMenager();
        CategoryMenager categoryMenager = new CategoryMenager();
        EvernoteUserMenager evernoteUserMenager = new EvernoteUserMenager();

        // GET: Home
        public ActionResult Index()
        {
            //categorycontroller üzerinden gelen view talebi
            //if (TempData["mm"]!=null)
            //{
            //    return View(TempData["mm"] as List<Note>);
            //}
            return View(noteMenager.ListQueryable().Where(x=>x.IsDraft==false).OrderByDescending(x => x.ModifiedOn).ToList());
            // return View(nm.GetAllNoteQueryable().OrderByDescending(x => x.ModifiedOn).ToList());
        }

        public ActionResult ByCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category cat = categoryMenager.Find(x=>x.Id==id.Value);
            if (cat == null)
            {
                return HttpNotFound();
                //  return RedirectToAction("","")
            }

            return View("Index", cat.Notes.Where(x=>x.IsDraft==false).OrderByDescending(x => x.ModifiedOn).ToList());
        }

        public ActionResult MostLiked()
        {
            return View("Index", noteMenager.ListQueryable().OrderByDescending(x => x.LikeCount).ToList());
        }

        public ActionResult About()
        {
            return View();
        }

        [Auth]
        public ActionResult ShowProfile()
        {
            BusinesLayerResult<EvernoteUser> res = evernoteUserMenager.GetUserById(CurrentSession.User.Id);

            if (res.Errors.Count > 0)
            {
                //kullanıcıyı hata ekranına yönlendirmemiz gerekiyor.

                ErrorViewModel errornotifyObj = new ErrorViewModel()
                {
                    Title = "Hata oluştu",
                    Items = res.Errors
                };

                return View("Error", errornotifyObj);
            }

            return View(res.Result);
        }

        [Auth]
        public ActionResult EditProfile()
        {
            BusinesLayerResult<EvernoteUser> res = evernoteUserMenager.GetUserById(CurrentSession.User.Id);

            if (res.Errors.Count > 0)
            {
                //kullanıcıyı hata ekranına yönlendirmemiz gerekiyor.

                ErrorViewModel errornotifyObj = new ErrorViewModel()
                {
                    Title = "Hata oluştu",
                    Items = res.Errors
                };

                return View("Error", errornotifyObj);
            }

            return View(res.Result);
        }

        [Auth]
        [HttpPost]
        public ActionResult EditProfile(EvernoteUser model, HttpPostedFileBase ProfileImage)
        {

            ModelState.Remove("ModifiedUsername");

            if (ModelState.IsValid)
            {
                if (ProfileImage != null &&
                    (ProfileImage.ContentType == "image/jpeg" ||
                     ProfileImage.ContentType == "image/JPEG" ||
                     ProfileImage.ContentType == "image/jpg" ||
                     ProfileImage.ContentType == "image/JPG" ||
                     ProfileImage.ContentType == "image/PNG" ||
                     ProfileImage.ContentType == "image/png"))
                {
                    string filename = $"user_{model.Id}.{ProfileImage.ContentType.Split('/')[1]}";

                    ProfileImage.SaveAs(Server.MapPath($"~/images/{filename}"));
                    model.ProfileImageFileName = filename;
                }
                BusinesLayerResult<EvernoteUser> res = evernoteUserMenager.UpdateProfile(model);

                if (res.Errors.Count > 0)
                {
                    ErrorViewModel errorNotifyObj = new ErrorViewModel()
                    {
                        Items = res.Errors,
                        Title = "Profil Güncellenemedi.",
                        RedirectingUrl = "/Home/EditProfile"
                    };

                    return View("Error", errorNotifyObj);
                }

                 // Profil güncellendiği için session güncellendi.
                CurrentSession.Set<EvernoteUser>("login", res.Result);

                //CurrentSession.Set<EvernoteUser>("login", res.Result);

                return RedirectToAction("ShowProfile");

            }
            return View(model);
        }

        [Auth]
        public ActionResult DeleteProfile()
        {
            BusinesLayerResult<EvernoteUser> res = evernoteUserMenager.RemoveUserById(CurrentSession.User.Id);

            if (res.Errors.Count > 0)
            {
                ErrorViewModel errorNotifyObj = new ErrorViewModel()
                {
                    Items = res.Errors,
                    Title = "Profil Silinemedi.",
                    RedirectingUrl = "/Home/ShowProfile"
                };

                return View("Error", errorNotifyObj);
            }

            evernoteUserMenager.Save();
            Session.Clear();

            return RedirectToAction("Index");
        }

        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                BusinesLayerResult<EvernoteUser> res = evernoteUserMenager.LoginUser(model);
                if (res.Errors.Count > 0)
                {
                    if (res.Errors.Find(x => x.Code == ErrorMessageCode.UserIsNotActive) != null)
                    {
                        ViewBag.SetLink = "E-Posta gönder";
                    }
                    res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                    return View(model);
                }

                CurrentSession.Set<EvernoteUser>("login",res.Result);      // Sessionda kullanıcı bilgileri saklama.
                return RedirectToAction("Index");   // Yönlendirme

            }

            return View(model);
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {

            if (ModelState.IsValid)
            {
                BusinesLayerResult<EvernoteUser> res = evernoteUserMenager.RegisterUser(model);

                if (res.Errors.Count > 0)
                {
                    res.Errors.ForEach(x => ModelState.AddModelError("", x.Message));
                    return View(model);
                }

                OkViewModel notifyObj = new OkViewModel()
                {
                    Title = "Kayıt Başarılı",
                    RedirectingUrl = "/Home/Login",
                };
                notifyObj.Items.Add("Lütfen e-posta adresine gönderddiğimiz aktivasyon linkine tıklayarak hesabımızı " +
                                    "aktive ediniz.Hesabınızı aktive etmeden not ekleyemez ve beğenme yapamazsınız.");
                return View("Ok",notifyObj);
            }
            return View(model);
        }

        public ActionResult UserActivate(Guid id)
        {
            BusinesLayerResult<EvernoteUser> res = evernoteUserMenager.ActivateUser(id);
            if (res.Errors.Count > 0)
            {
                ErrorViewModel notifyObj = new ErrorViewModel()
                {
                    Title = "Geçersiz İşlem",
                    Items = res.Errors
                };
                TempData["errors"] = res.Errors;
                return View("Error",notifyObj);
            }
            OkViewModel notifyObjOk=new OkViewModel()
            {
                Title="Hesap Aktifleştirildi",
                RedirectingUrl = "/Home/Login"
            };
            notifyObjOk.Items.Add("Hesabınız aktifleştirildi. Artık not paylaşımı ve beğenme yapabilirsiniz.");
            return View("Ok",notifyObjOk);
        }

        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Index");
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        public ActionResult HasError()
        {
            return View();
        }

    }
}