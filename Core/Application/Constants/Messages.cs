using Domain.Entities;
using System.Runtime.Serialization;

namespace Application.Constants
{
    public static class Messages
    {
        public static string JobPositionAdded = "İş pozisyonu eklendi.";
        public static string JobPositionDeleted = "İş pozisyonu silindi.";
        public static string JobPositionUpdated = "İş pozisyonu güncellendi.";
        public static string UserRegistered = "Kullanıcı kayıt edildi.";
        public static string UserDeleted = "Kullanıcı silindi.";
        public static string UnknownError = "Bilinmeyen hata.";
        public static string UserUpdated = "Kullanıcı bilgileri güncellendi";
        public static string UserNotFound = "Kullanıcı bulunamadı.";
        public static string PasswordError = "Bilgileriniz hatalı.";
        public static string SuccessfulLogin = "Giriş başarılı.";
        public static string UserAlreadyExists = "Kullanıcı daha önce kayıt olmuş.";
        public static string AuthorizationDenied = "Yetkiniz yok.";
        public static string JobPositionExists = "İş pozisyonu daha önce eklenmiş";
        public static string EmployerAdded = "İş veren eklendi.";
        public static string EmployerDeleted = "İş veren silindi.";
        public static string EmployerUpdated = "İş veren güncellendi.";
        public static string EmployerExists = "İş veren daha önce eklenmiş.";
        public static string SystemStaffAdded = "Sistem çalışanı eklendi.";
        public static string SystemStaffDeleted = "Sistem çalışanı silindi.";
        public static string SystemStaffUpdated = "Sistem çalışanının bilgileri güncellendi.";
        public static string JobAdvertisementAdded = "İş ilanı eklendi.";
        public static string JobAdvertisementDeleted = "İş ilanı silindi.";
        public static string JobAdvertisementExists = "İş ilanı daha önce eklenmiş";
        public static string JobAdvertisementUpdated = "İş ilanı güncellendi.";
    }
}