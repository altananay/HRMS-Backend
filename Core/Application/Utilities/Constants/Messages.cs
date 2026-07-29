namespace Application.Utilities.Constants
{
    public static class Messages
    {
        public static class Contact
        {
            public const string Added = "Mesajınız alındı.";
            public const string Updated = "İletişim kaydı güncellendi.";
            public const string Deleted = "İletişim kaydı silindi.";
            public const string NotFound = "İletişim kaydı bulunamadı.";
        }

        public static class Cv
        {
            public const string Added = "CV oluşturuldu.";
            public const string Updated = "CV güncellendi.";
            public const string Deleted = "CV silindi.";
            public const string NotFound = "CV bulunamadı.";
            public const string AlreadyExists = "Bu iş arayana ait bir CV zaten mevcut.";
        }

        public static class Employer
        {
            public const string Updated = "İşveren bilgileri güncellendi.";
            public const string Deleted = "İşveren silindi.";
            public const string NotFound = "İşveren bulunamadı.";
        }

        public static class JobSeeker
        {
            public const string Updated = "İş arayan bilgileri güncellendi.";
            public const string Deleted = "İş arayan silindi.";
            public const string NotFound = "İş arayan bulunamadı.";
        }

        public static class SystemStaff
        {
            public const string Added = "Personel eklendi.";
            public const string Updated = "Personel güncellendi.";
            public const string Deleted = "Personel silindi.";
            public const string NotFound = "Personel bulunamadı.";
        }

        public static class JobAdvertisement
        {
            public const string Added = "İş ilanı yayınlandı.";
            public const string Updated = "İş ilanı güncellendi.";
            public const string Deleted = "İş ilanı silindi.";
            public const string NotFound = "İş ilanı bulunamadı.";
        }

        public static class JobApplication
        {
            public const string Added = "İş başvurusu yapıldı.";
            public const string Updated = "İş başvurusu güncellendi.";
            public const string Deleted = "İş başvurusu silindi.";
            public const string NotFound = "İş başvurusu bulunamadı.";
            public const string AlreadyApplied = "Bu ilana zaten başvurdunuz.";
        }

        public static class JobPosition
        {
            public const string Added = "Pozisyon eklendi.";
            public const string Updated = "Pozisyon güncellendi.";
            public const string Deleted = "Pozisyon silindi.";
            public const string NotFound = "Pozisyon bulunamadı.";
            public const string InUse = "Bu pozisyon kullanımda olduğu için silinemez.";
        }

        public static class Authentication
        {
            public const string InvalidCredentials = "E-posta veya parola hatalı.";

            public const string EmailAlreadyUsed = "Bu e-posta adresi kullanılamaz.";
            public const string Registered = "Kayıt işlemi başarılı.";
            public const string LoggedIn = "Giriş başarılı.";
            public const string LoggedOut = "Çıkış yapıldı.";
            public const string InvalidRefreshToken = "Oturum geçersiz. Lütfen tekrar giriş yapın.";
            public const string PasswordChanged = "Parolanız güncellendi.";
            public const string AuthorizationDenied = "Bu işlem için yetkiniz yok.";

            /// <summary>
            /// Returned whether or not the address is registered — the response must not reveal
            /// which, for the same reason sign-in answers a uniform 401.
            /// </summary>
            public const string PasswordResetRequested =
                "Adres kayıtlıysa parola sıfırlama bağlantısı gönderildi.";

            public const string PasswordResetCompleted =
                "Parolanız sıfırlandı. Yeni parolanızla giriş yapabilirsiniz.";

            public const string InvalidPasswordResetToken =
                "Bağlantı geçersiz veya süresi dolmuş. Lütfen yeni bir sıfırlama talebi oluşturun.";
        }
    }
}
