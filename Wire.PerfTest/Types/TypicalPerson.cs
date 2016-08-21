using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Wire.PerfTest.Types
{
    [DataContract]
    public enum MaritalStatus
    {
        [EnumMember]
        Married,
        [EnumMember]
        Divorced,
        [EnumMember]
        HatesAll
    }

    [ProtoContract]
    [DataContract]
    [Serializable]
    public class TypicalPersonData
    {
        /// <summary>
        /// Required by some serilizers (i.e. XML)
        /// </summary>
        public TypicalPersonData() { }


        [ProtoMember(1)]
        [DataMember]
        public string Address1;

        [ProtoMember(2)]
        [DataMember]
        public string Address2;

        [ProtoMember(3)]
        [DataMember]
        public string AddressCity;

        [ProtoMember(4)]
        [DataMember]
        public string AddressState;

        [ProtoMember(5)]
        [DataMember]
        public string AddressZip;

        [ProtoMember(6)]
        [DataMember]
        public double CreditScore;

        [ProtoMember(7)]
        [DataMember]
        public DateTime DOB;

        [ProtoMember(8)]
        [DataMember]
        public string EMail;

        [ProtoMember(9)]
        [DataMember]
        public string FirstName;

        [ProtoMember(10)]
        [DataMember]
        public string HomePhone;

        [ProtoMember(11)]
        [DataMember]
        public string LastName;

        [ProtoMember(12)]
        [DataMember]
        public MaritalStatus MaritalStatus;

        [ProtoMember(13)]
        [DataMember]
        public string MiddleName;

        [ProtoMember(14)]
        [DataMember]
        public string MobilePhone;

        [ProtoMember(15)]
        [DataMember]
        public bool RegisteredToVote;

        [ProtoMember(16)]
        [DataMember]
        public decimal Salary;

        [ProtoMember(17)]
        [DataMember]
        public int YearsOfService;



        [ProtoMember(18)]
        [DataMember]
        public string SkypeID;
        [ProtoMember(19)]
        [DataMember]
        public string YahooID;
        [ProtoMember(20)]
        [DataMember]
        public string GoogleID;

        [ProtoMember(21)]
        [DataMember]
        public string Notes;

        [ProtoMember(22)]
        [DataMember]
        public bool? IsSmoker;
        [ProtoMember(23)]
        [DataMember]
        public bool? IsLoving;
        [ProtoMember(24)]
        [DataMember]
        public bool? IsLoved;
        [ProtoMember(25)]
        [DataMember]
        public bool? IsDangerous;
        [ProtoMember(26)]
        [DataMember]
        public bool? IsEducated;
        [ProtoMember(27)]
        [DataMember]
        public DateTime? LastSmokingDate;

        [ProtoMember(28)]
        [DataMember]
        public decimal? DesiredSalary;
        [ProtoMember(29)]
        [DataMember]
        public double? ProbabilityOfSpaceFlight;

        [ProtoMember(30)]
        [DataMember]
        public int? CurrentFriendCount;
        [ProtoMember(31)]
        [DataMember]
        public int? DesiredFriendCount;


        public static TypicalPersonData MakeRandom()
        {
            var rnd = 123;

            var data = new TypicalPersonData
            {
                FirstName = NaturalTextGenerator.GenerateFirstName(),
                MiddleName =NaturalTextGenerator.GenerateFirstName(),
                LastName = NaturalTextGenerator.GenerateLastName(),
                DOB = DateTime.Now.AddYears(5),
                Salary = 55435345,
                YearsOfService = 25,
                CreditScore = 0.7562,
                RegisteredToVote = (DateTime.UtcNow.Ticks & 1) == 0,
                MaritalStatus = MaritalStatus.HatesAll,
                Address1 = NaturalTextGenerator.GenerateAddressLine(),
                Address2 = NaturalTextGenerator.GenerateAddressLine(),
                AddressCity = NaturalTextGenerator.GenerateCityName(),
                AddressState = "CA",
                AddressZip = "91606",
                HomePhone = (DateTime.UtcNow.Ticks & 1) == 0 ? "(555) 123-4567" : null,
                EMail = NaturalTextGenerator.GenerateEMail()
            };

            if (0 != (rnd & (1 << 32))) data.Notes = NaturalTextGenerator.Generate(45);
            if (0 != (rnd & (1 << 31))) data.SkypeID = NaturalTextGenerator.GenerateEMail();
            if (0 != (rnd & (1 << 30))) data.YahooID = NaturalTextGenerator.GenerateEMail();

            if (0 != (rnd & (1 << 29))) data.IsSmoker = 0 != (rnd & (1 << 17));
            if (0 != (rnd & (1 << 28))) data.IsLoving = 0 != (rnd & (1 << 16));
            if (0 != (rnd & (1 << 27))) data.IsLoved = 0 != (rnd & (1 << 15));
            if (0 != (rnd & (1 << 26))) data.IsDangerous = 0 != (rnd & (1 << 14));
            if (0 != (rnd & (1 << 25))) data.IsEducated = 0 != (rnd & (1 << 13));

            if (0 != (rnd & (1 << 24))) data.LastSmokingDate = DateTime.Now.AddYears(-10);


            if (0 != (rnd & (1 << 23))) data.DesiredSalary = rnd / 1000m;
            if (0 != (rnd & (1 << 22))) data.ProbabilityOfSpaceFlight = rnd / (double)int.MaxValue;

            if (0 != (rnd & (1 << 21)))
            {
                data.CurrentFriendCount = rnd % 123;
                data.DesiredFriendCount = rnd % 121000;
            }

            return data;
        }
    }

    public class NaturalTextGenerator
    {
        public static string GenerateEMail()
        {
            return "foo@fooo.com";
        }

        public static string Generate(int i)
        {
            return "fskldjflksjfl ksj dlfkjsdfl ksdjklf jsdlkj";
        }

        public static string GenerateAddressLine()
        {
            return "fkjdskfjskfjs";
        }

        public static string GenerateFirstName()
        {
            return "fksjdfkjsdkfjksdfs";
        }

        public static string GenerateCityName()
        {
            return "fksdfkjsdkfjsdkfs";
        }

        public static string GenerateLastName()
        {
            return "kfjdskdfjskj";
        }
    }

    public class ExternalRandomGenerator
    {
    }
}
