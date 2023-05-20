﻿using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using BusinessModel;
using DataAccess;

namespace BusinessLogic
{
	public class SiginLogic : ISigninStatus,IIsLoginDataValid
	{
		private readonly ConnectionSettings _connection;
        private readonly ParamSignInDataModels? _signinData;
        private static Random random = new Random((int)DateTime.Now.Ticks);

        public SiginLogic(IOptions<ConnectionSettings> connection, ParamSignInDataModels signinData)
        {
            _connection = connection.Value;
            _signinData = signinData;
        }

       
        async public Task<bool> IsLoginDataValid()
        {
            //checked if username exist in database
            ReturnGetSigninDataModel dataAccessData = await GetSigninStatus();
            if (!dataAccessData.IsUsernameExist) return false;

            //begin build hash password from users input and salt from database
            var saltFromDB = dataAccessData.Salt;
            var passwordFromUser = _signinData.IStillLoveYou;
            var stringPasswordPlusSalt = passwordFromUser + saltFromDB;

            //begin hashing
            var sha = SHA512Managed.Create();
            //convert password to bytes
            var bytesPasswordWithSalt = Encoding.ASCII.GetBytes(stringPasswordPlusSalt);
            //compute hash value
            var hashPasswordWithSalt = sha.ComputeHash(bytesPasswordWithSalt);
            //convert hash to string
            var hashStringFormatPasswordWithSalt = GetStringFromHash(hashPasswordWithSalt);

            //get the hash password with salt from database
            var hashPasswordWithSaltFormDB = dataAccessData.HashIStillLoveYouWithSalt;

            //Compare the 2 hashPasswordString
            if (hashStringFormatPasswordWithSalt != hashPasswordWithSaltFormDB) return false;
            
            return true;
        }

        async public Task<ReturnGetSigninDataModel> GetSigninStatus()
        {
            GetSigninDataAccess dataAccess = new GetSigninDataAccess(_connection, _signinData);
            return await dataAccess.GetSigninStatus();
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

      


        //private string RandomString(int size)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    char ch;
        //    for (int i = 0; i < size; i++)
        //    {
        //        ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
        //        builder.Append(ch);
        //    }

        //    return builder.ToString();
        //}

    }
}