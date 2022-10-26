using Application.Dtos;
namespace Application.Abstractions
{
    public interface ICheckPersonService
    {
        bool CheckIfRealPerson(MernisCheckDto mernisCheckDto);
        bool CheckPerson();
    }
}