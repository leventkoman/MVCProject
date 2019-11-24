﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyEvernote.Entities.ValueObjects
{
    public class RegisterViewModel
    {

        [DisplayName("Kullanıcı adı"),
        Required(ErrorMessage = "{0} alanı boş geçilemez."),
        StringLength(25,ErrorMessage = "{0} en fazla {1} karakter olmalı.")]
        public string Username { get; set; }

        [DisplayName("EPosta"),
        Required(ErrorMessage = "{0} alanı boş geçilemez."),
        StringLength(75, ErrorMessage = "{0} en fazla {1} karakter olmalı."),
        EmailAddress(ErrorMessage = "{0} alanı için lütfen geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [DisplayName("Şifre"),
        Required(ErrorMessage = "{0} alanı boş geçilemez."),
        DataType(DataType.Password),
        StringLength(25, ErrorMessage = "{0} en fazla {1} karakter olmalı.")]
        public string Password { get; set; }

        [DisplayName("Şifre Tekrarı"),
        Required(ErrorMessage = "{0} alanı boş geçilemez."),
        DataType(DataType.Password),
        StringLength(25,ErrorMessage = "{0} en fazla {1} karakter olmalı.")
        Compare("Password", ErrorMessage="{0} ie {1} uyuşmuyor.")]
        public string RePassword { get; set; }
    }
}