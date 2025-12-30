using System;

namespace BodyTracker.Models
{

    /// <summary>
    /// Represents the structure of a human person data
    /// </summary
    public class PersonModel
    {
        /// <summary>
        /// Contains the PersonModel ID from the database table
        /// </summary>
        public int PersonID { get; set; }

        /// <summary>
        /// Contains the persons first name
        /// </summary>
        public string PersonFirstName { get; set; } = string.Empty;

        /// <summary>
        /// Contains the persons last name
        /// </summary>
        public string PersonLastName { get; set; } = string.Empty;

        /// <summary>
        /// Contains the persons birth date
        /// </summary>
        public DateTime? PersonBirthDate { get; set; }

        /// <summary>
        /// Creates a string with the first and last name.
        /// </summary>
        /// <returns>First Name + Lastname</returns>
        public override string ToString()
        { 
            return PersonFirstName + " " + PersonLastName;
        }
    }
}
