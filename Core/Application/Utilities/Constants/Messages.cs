namespace Application.Utilities.Constants
{
    public static class Messages
    {
        public static class JobPosition
        {
            public static string JobPositionAdded = "İş pozisyonu eklendi.";
            public static string JobPositionDeleted = "İş pozisyonu silindi.";
            public static string JobPositionUpdated = "İş pozisyonu güncellendi.";
            public static string JobPositionExists = "İş pozisyonu daha önce eklenmiş";
            public static string JobPositionNotExists = "İş pozisyonu bulunamadı";
        }

        public static class User
        {
            public static string UserRegistered = "Kullanıcı kayıt edildi.";
            public static string UserDeleted = "Kullanıcı silindi.";
            public static string UserUpdated = "Kullanıcı bilgileri güncellendi";
            public static string UserNotFound = "Kullanıcı bulunamadı.";
        }

        public static class Authentication
        {
            public static string PasswordError = "Bilgileriniz hatalı.";
            public static string SuccessfulLogin = "Giriş başarılı.";
            public static string UserAlreadyExists = "Kullanıcı daha önce kayıt olmuş.";
            public static string AuthorizationDenied = "Yetkiniz yok.";
        }

        public static class Employer
        {
            public static string EmployerAdded = "İş veren eklendi.";
            public static string EmployerDeleted = "İş veren silindi.";
            public static string EmployerUpdated = "İş veren güncellendi.";
            public static string EmployerExists = "İş veren daha önce eklenmiş.";
            public static string EmployerNotExists = "İş veren bulunamadı";
        }

        public static class SystemStaff
        {
            public static string SystemStaffAdded = "Sistem çalışanı eklendi.";
            public static string SystemStaffDeleted = "Sistem çalışanı silindi.";
            public static string SystemStaffUpdated = "Sistem çalışanının bilgileri güncellendi.";
            public static string SystemStaffNotExists = "Sistem çalışanı bulunamadı";
            public static string SystemStaffNotExistsByEmail = "Bu emaile sahip sistem çalışanı bulunamadı";
        }

        public static class JobAdvertisement
        {
            public static string JobAdvertisementAdded = "İş ilanı eklendi.";
            public static string JobAdvertisementDeleted = "İş ilanı silindi.";
            public static string JobAdvertisementExists = "İş ilanı daha önce eklenmiş";
            public static string JobAdvertisementNotExists = "İş ilanı bulunamadı";
            public static string JobAdvertisementUpdated = "İş ilanı güncellendi.";
        }


        public static class Cv
        {
            public static string CvUpdated = "Cv güncellendi";
            public static string CvDeleted = "Cv silindi";
            public static string CvAdded = "Cv eklendi";
            public static string CvExists = "Daha önce cv eklenmiş.";
            public static string CvNotExists = "Cv bulunamadı";
            public static string JobSeekerCvNotExists = "Bu kullanıcıya ait Cv bulunamadı";
        }

        public static class Mernis
        {
            public static string CitizenError = "Hatalı vatandaş bilgisi.";
            public static string CitizenSuccessful = "Vatandaş bilgisi doğru.";
            public static string NationalityIdExists = "Bu TC kimlik numarasıyla daha önce kullanıcı kayıt edilmiş";
        }

        public static class JobApplication
        {
            public static string JobApplicationMade = "İş başvurusu yapıldı.";
            public static string JobApplicationDeleted = "İş başvurusu silindi.";
            public static string JobApplicationUpdated = "İş başvurusu güncellendi.";
            public static string JobApplicationNotFound = "İş başvurusu bulunamadı";
        }

        public static class Contact
        {
            public static string ContactAdded = "Mesajınız gönderildi.";
            public static string ContactDeleted = "Mesaj silindi.";
            public static string ContactUpdated = "Mesaj güncellendi.";
            public static string ContactNotExists = "Mesaj bulunamadı.";
        }

        public static class JobSeeker
        {
            public static string JobSeekerNotExists = "Şirket bulunamadı";
            public static string JobSeekerEmailNotExists = "Şirkete ait email bulunamadı";
        }

        public static class Common
        {
            public static string UnknownError = "Bilinmeyen hata.";
        }
    }
}