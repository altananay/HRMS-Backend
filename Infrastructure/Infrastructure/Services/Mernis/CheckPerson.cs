using Application.Abstractions;
using Application.Utilities.Dtos;
using MernisServiceReference;

namespace Infrastructure.Services.Mernis
{
    public class CheckPerson : ICheckPersonService
    {
        public bool CheckIfRealPerson(MernisCheckDto userCheck)
        {
            KPSPublicSoapClient kPSPublicSoapClient = new KPSPublicSoapClient(KPSPublicSoapClient.EndpointConfiguration.KPSPublicSoap);
            return kPSPublicSoapClient.TCKimlikNoDogrulaAsync(new TCKimlikNoDogrulaRequest(new TCKimlikNoDogrulaRequestBody(long.Parse(userCheck.NationalityId), userCheck.FirstName.ToUpper(), userCheck.LastName.ToUpper(), userCheck.DateOfBirth.Year))).Result.Body.TCKimlikNoDogrulaResult;
        }

        bool ICheckPersonService.CheckPerson()
        {
            return true;
        }
    }
}