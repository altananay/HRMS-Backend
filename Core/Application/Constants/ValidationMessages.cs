namespace Application.Constants
{
    public static class ValidationMessages
    {
        //education validation messages
        public static string SchoolCantEmpty = "Okul bilgisi boş bırakılamaz";
        public static string GraduateCantEmpty = "Mezuniyet bilgisi boş bırakılamaz";
        public static string YearsCantEmpty = "Yıl bilgisi boş bırakılamaz";
        public static string MajorCantEmpty = "Bölüm bilgisi boş bırakılamaz";

        //Job experience validation messages
        public static string JobPositionCantEmpty = "İş pozisyonu bilgisi boş bırakılamaz";
        public static string JobPositionLength = "İş pozisyonu 5 karakterden az olamaz.";
        public static string CompanyNameInformationCantEmpty = "Şirket ismi boş bırakılamaz";
        public static string DepartmentCantEmpty = "Departman boş bırakılamaz";

        //Programming language validation message
        public static string ProgrammingLanguageCantEmpty = "Programlama dili boş bırakılamaz.";

        //library and framework validationm message
        public static string LibraryAndFrameworkCantEmpty = "Kütüphane ve framework bilgileri boş bırakılamaz";

        //language validation message
        public static string LanguagesCantEmpty = "Yabancı dil bilgisi boş bırakılamaz";

        //social media validation messages
        public static string GithubCantEmpty = "Github adresinizi paylaşmalısınız";
        public static string LinkedinCantEmpty = "Linkedin adresinizi paylaşmalısınız";

        //Hobby validation message
        public static string HobbiesCantEmpty = "Hobiler boş bırakılamaz";

        //Employer
        public static string CompanyNameCantEmpty = "Şirket adı boş bırakılamaz.";
        public static string WebSiteCantEmpty = "Web site ismi boş bırakılamaz.";
        public static string PhoneNumberCantEmpty = "Telefon numarası boş bırakılamaz.";

        //common
        public static string LevelCantEmpty = "Seviye boş bırakılamaz";
        public static string ObjectIdValidationError = "Bilgiyi doğru formatta gönderin.";
        public static string EmailCantEmpty = "Email boş bırakılamaz.";

        //auth
        public static string EmailFormat = "Email bilgisini doğru formatta girin.";
        public static string PasswordCantEmpty = "Şifre boş olamaz";

        //JobAdvertisement
        public static string DeadlineCantEmpty = "Son başvuru tarihi boş olamaz.";
        public static string JobTypeCantEmpty = "İş türü boş olamaz.";
        public static string MinSalaryCantEmpty = "Minimim maaş bilgisi boş olamaz.";
        public static string MaxSalaryCantEmpty = "Maximum maaş bilgisi boş olamaz.";
        public static string OpenPositionCantEmpty = "Açık pozisyon sayısı boş olamaz.";
        public static string MinSalaryCantBeGreaterThanMaxSalary = "Minimum maaş maksimum maaştan büyük olamaz.";
        public static string MaxSalaryCantBeLessThanMinSalary = "Maximum maaş minimum maaştan küçük olamaz.";

        //JobApplication
        public static string JobApplicationResultCantEmpty = "İş başvurusu sonucu boş olamaz.";

        //Register
        public static string FirstNameCantBeEmpty = "İsim boş olamaz.";
        public static string LastNameCantBeEmpty = "Soyadı boş olamaz.";

        //SystemStaff
        public static string ClaimCantBeEmpty = "Lütfen yetki verin.";
        public static string ClaimFormat = "Yetkiyi doğru formatta verin.";

        //Contact
        public static string MessageLength = "Mesajınız 20 ile 200 karakter arasında olmalı.";
        public static string SubjectLength = "Konu başlığınız 5 ile 100 karakter arasında olmalı";
    }
}