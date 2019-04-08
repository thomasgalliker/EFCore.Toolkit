using System;
using System.Diagnostics;
using EFCore.Toolkit.Abstractions.Auditing;

namespace ToolkitSample.Model
{
    [DebuggerDisplay("Person: Id={Id}, FirstName={FirstName}, LastName={LastName}")]
    public class Person : ICreatedDate, IUpdatedDate
    {
        public int Id { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public DateTime Birthdate { get; set; }

        public string CountryId { get; set; }

        public Country Country { get; set; }

        public byte[] RowVersion { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}