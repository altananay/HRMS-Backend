﻿using MongoDB.Bson;

namespace Application.CrossCuttingConcerns.Validation.Validators.CustomValidateRules
{
    public static class ObjectIdValidation
    {
        public static bool ObjectIdValidate(string arg)
        {
            if (ObjectId.TryParse(arg, out _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}