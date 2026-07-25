namespace Application.Utilities.Constants
{
    /// <summary>
    /// User-facing messages. Turkish, matching the existing API surface.
    /// </summary>
    /// <remarks>
    /// Rewritten as <c>const</c> rather than mutable <c>static string</c> fields — the previous
    /// version could be reassigned at runtime by any caller.
    ///
    /// Note that authentication failures now share a single
    /// <see cref="Authentication.InvalidCredentials"/> message. The old code returned
    /// "Bilgileriniz hatalı." for a wrong password but let an unknown email fall through to a
    /// BusinessException with a different message <i>and</i> a different status code, which together
    /// made a reliable user-enumeration oracle.
    /// </remarks>
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
            /// <summary>Deliberately identical for unknown email, wrong password and disabled account.</summary>
            public const string InvalidCredentials = "E-posta veya parola hatalı.";

            public const string EmailAlreadyUsed = "Bu e-posta adresi kullanılamaz.";
            public const string Registered = "Kayıt işlemi başarılı.";
            public const string LoggedIn = "Giriş başarılı.";
            public const string LoggedOut = "Çıkış yapıldı.";
            public const string InvalidRefreshToken = "Oturum geçersiz. Lütfen tekrar giriş yapın.";
            public const string PasswordChanged = "Parolanız güncellendi.";
            public const string AuthorizationDenied = "Bu işlem için yetkiniz yok.";
        }
    }
}
