using MyEvernote.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyEvernote.BusinessLayer.Abstract;
using MyEvernote.Common.Helpers;
using MyEvernote.DataAccessLayer.EntityFramework;
using MyEvernote.Entities.Messages;
using MyEvernote.Entities.ValueObjects;
using MyEvernote.BusinessLayer.Results;

namespace MyEvernote.BusinessLayer
{
   public class EvernoteUserMenager : MenagerBase<EvernoteUser>
    {

        public BusinesLayerResult<EvernoteUser> RegisterUser(RegisterViewModel data)
        {
            /*Yapılacak işlemler aşağıdaki gibidir.
          1-  Kullanıcı user kontrolü
          2-  Kullanıcı eposta kontrolü
          3-  Kayıt işlemi
          4-  Aktivasyon e-postası gönderimi.*/

            EvernoteUser user = Find(x => x.Username == data.Username || x.Email==data.Email);
            BusinesLayerResult<EvernoteUser> res=new BusinesLayerResult<EvernoteUser>();
            if (user!=null)
            {
                if (user.Username == data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAllreadyExists, "Kullanıcı adı kayıtlı.");
                }

                if (user.Email == data.Email)
                {
                    res.AddError(ErrorMessageCode.EmailAllreadyExists, "E-posta adresi kayıtlı.");
                }
            }

            else
            {
                int dbResult =base.Insert(new EvernoteUser()

                {
                    Username = data.Username,
                    Email = data.Email,
                    ProfileImageFileName = "images.jpg",
                    Password = data.Password,
                    ActivateGuid = Guid.NewGuid(),
                    IsActive = false,
                    IsAdmin = false

                });

                if (dbResult > 0)
                {
                    res.Result = Find(x => x.Email == data.Email && x.Username == data.Username);
                    string siteUri = ConfigHelper.Get<string>("SiteRootUri");
                    string activateUri = $"{siteUri}/Home/UserActivate/{res.Result.ActivateGuid}";
                    string body = $"Merhaba {res.Result.Username};<br><br>Hesabınızı aktifleştirmek için <a href='{activateUri}' target='_blank'>tıklayınız</a>.";
                    MailHelper.SendMail(body,res.Result.Email,"MyEvernote Hesap Aktifleştirme");
                }
            }

            return res;
        }

        public BusinesLayerResult<EvernoteUser> GetUserById(int id)
        {
            BusinesLayerResult<EvernoteUser> res=new BusinesLayerResult<EvernoteUser>();
            res.Result = Find(x => x.Id == id);

            if (res.Result==null)
            {
                res.AddError(ErrorMessageCode.UserNotFound,"Kullanıcı bulunamadı.");
            }

            return res;
        }

        public BusinesLayerResult<EvernoteUser> LoginUser(LoginViewModel data)
        {
            /* Yapılacak işlemler aşağıdaki gibidir.
          1-  Giriş kontrolü
          2-  Hesap aktive edilmiş mi? */


            BusinesLayerResult<EvernoteUser> res = new BusinesLayerResult<EvernoteUser>();
            res.Result = Find(x => x.Username == data.Username && x.Password == data.Password);

            if (res.Result != null )
            {
                if (!res.Result.IsActive)
                {
                    res.AddError(ErrorMessageCode.UserIsNotActive, "Kullanıcı aktifleştirilmemiştir.");
                    res.AddError(ErrorMessageCode.CheckYourEmail, "Lütfen e-posta adresini kontrol ediniz.");
                }
            }
            else
            {
                res.AddError(ErrorMessageCode.UsernameOrPassWrong, "Kullanıcı adı ya da şifre uyuşmuyor");
            }

            return res;
        }

        public BusinesLayerResult<EvernoteUser> ActivateUser(Guid activateId)
        {
            BusinesLayerResult<EvernoteUser> res = new BusinesLayerResult<EvernoteUser>();
            res.Result = Find(x => x.ActivateGuid==activateId);
            if (res.Result!=null)
            {
                if (res.Result.IsActive)
                {
                    res.AddError(ErrorMessageCode.UserAreadyActive,"Kullanıcı zaten aktif edilmiştir.");
                    return res;
                }

                res.Result.IsActive = true;
                Update(res.Result);
            }
            else
            {
                res.AddError(ErrorMessageCode.ActivateIdDoesNotExists, "Aktifleştirilecek kullanıcı bulunamadı.");
            }

            return res;
        }

        public BusinesLayerResult<EvernoteUser> UpdateProfile(EvernoteUser data)
        {
            EvernoteUser db_user =Find(x=>x.Username == data.Username || x.Email == data.Email);
            BusinesLayerResult<EvernoteUser> res = new BusinesLayerResult<EvernoteUser>();

            if (db_user != null && db_user.Id != data.Id)
            {
                if (db_user.Username == data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAllreadyExists, "Kullanıcı adı kayıtlı.");
                }

                if (db_user.Email == data.Email)
                {
                    res.AddError(ErrorMessageCode.EmailAllreadyExists, "E-posta adresi kayıtlı.");
                }

                return res;
            }

            res.Result =Find(x => x.Id == data.Id);
            res.Result.Email = data.Email;
            res.Result.Name = data.Name;
            res.Result.Surname = data.Surname;
            res.Result.Password = data.Password;
            res.Result.Username = data.Username;

            if (string.IsNullOrEmpty(data.ProfileImageFileName) == false)
            {
                res.Result.ProfileImageFileName = data.ProfileImageFileName;
            }

            if (base.Update(res.Result) == 0)
            {
                res.AddError(ErrorMessageCode.ProfileCouldNotUpdated, "Profil güncellenemedi.");
            }

            return res;
        }

        public BusinesLayerResult<EvernoteUser> RemoveUserById(int id)
        {
            BusinesLayerResult<EvernoteUser> res = new BusinesLayerResult<EvernoteUser>();
            EvernoteUser user =Find(x => x.Id == id);

            if (user != null)
            {
                if (Delete(user) == 0)
                {
                    res.AddError(ErrorMessageCode.UserCouldNotRemove, "Kullanıcı silinemedi.");
                    return res;
                }
            }
            else
            {
                res.AddError(ErrorMessageCode.UserCouldNotInserted, "Kullanıcı bulunamadı.");
            }

            return res;
        }

        //metot gizleme new ile olur.
        public new BusinesLayerResult<EvernoteUser> Insert(EvernoteUser data)
        {
            EvernoteUser user = Find(x => x.Username == data.Username || x.Email == data.Email);
            BusinesLayerResult<EvernoteUser> res = new BusinesLayerResult<EvernoteUser>();
            res.Result = data;

            if (user != null)
            {
                if (user.Username == data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAllreadyExists, "Kullanıcı adı kayıtlı.");
                }

                if (user.Email == data.Email)
                {
                    res.AddError(ErrorMessageCode.EmailAllreadyExists, "E-posta adresi kayıtlı.");
                }
            }

            else
            {
                res.Result.ProfileImageFileName = "images.jpg";
                res.Result.ActivateGuid = Guid.NewGuid();
                if (base.Insert(res.Result)==0)
                {
                    res.AddError(ErrorMessageCode.UserCouldNotInserted, "Kullanıcı eklenemedi.");
                }

            }

            return res;
        }

        public new BusinesLayerResult<EvernoteUser> Update(EvernoteUser data)
        {
            EvernoteUser db_user = Find(x => x.Username == data.Username || x.Email == data.Email);
            BusinesLayerResult<EvernoteUser> res = new BusinesLayerResult<EvernoteUser>();
            res.Result = data;

            if (db_user != null && db_user.Id != data.Id)
            {
                if (db_user.Username == data.Username)
                {
                    res.AddError(ErrorMessageCode.UsernameAllreadyExists, "Kullanıcı adı kayıtlı.");
                }

                if (db_user.Email == data.Email)
                {
                    res.AddError(ErrorMessageCode.EmailAllreadyExists, "E-posta adresi kayıtlı.");
                }

                return res;
            }

            res.Result = Find(x => x.Id == data.Id);
            res.Result.Email = data.Email;
            res.Result.Name = data.Name;
            res.Result.Surname = data.Surname;
            res.Result.Password = data.Password;
            res.Result.Username = data.Username;
            res.Result.IsActive = data.IsActive;
            res.Result.IsAdmin = data.IsAdmin;

            if (string.IsNullOrEmpty(data.ProfileImageFileName) == false)
            {
                res.Result.ProfileImageFileName = data.ProfileImageFileName;
            }

            if (base.Update(res.Result) == 0)
            {
                res.AddError(ErrorMessageCode.UserCouldNotUpdated, "Kullanıcı güncellenemedi.");
            }

            return res;
        }
    }
}
