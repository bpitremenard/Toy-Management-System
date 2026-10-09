using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests.Toys
{
    public class ToyModelTests
    {
        private static List<ValidationResult> ValidateModel(Toy toy)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(toy, new ValidationContext(toy), results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Toy_WithValidData_PassesValidation()
        {
            var toy = new Toy
            {
                Name = "Building Blocks",
                Description = "A set of wooden blocks.",
                AdditionalInformation = "Includes multiple pieces.",
                Category = "Cognitive",
                Material = "Wood",
                LocationCode = "B09"
            };

            var results = ValidateModel(toy);

            Assert.Empty(results);
        }

        [Fact]
        public void Toy_WithBlankName_FailsValidation()
        {
            var toy = new Toy
            {
                Name = "   ",
                LocationCode = "A01"
            };

            var results = ValidateModel(toy);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Toy.Name)));
        }

        [Fact]
        public void Toy_WithTooLongDescription_FailsValidation()
        {
            var toy = new Toy
            {
                Name = "Toy Car",
                LocationCode = "A01",
                Description = new string('d', 2001)
            };

            var results = ValidateModel(toy);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Toy.Description)));
        }

        [Fact]
        public void Toy_WithTooLongAdditionalInformation_FailsValidation()
        {
            var toy = new Toy
            {
                Name = "Toy Car",
                LocationCode = "A01",
                AdditionalInformation = new string('a', 2001)
            };

            var results = ValidateModel(toy);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Toy.AdditionalInformation)));
        }

        [Fact]
        public void Toy_WithInvalidLocationCode_FailsValidation()
        {
            var toy = new Toy
            {
                Name = "Toy Car",
                LocationCode = "A 01!"
            };

            var results = ValidateModel(toy);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Toy.LocationCode)));
        }

        [Fact]
        public void Toy_WithUnsupportedMaterial_FailsValidation()
        {
            var toy = new Toy
            {
                Name = "Toy Car",
                LocationCode = "A01",
                Material = "Glass"
            };

            var results = ValidateModel(toy);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Toy.Material)));
        }

        [Fact]
        public void Toy_WithUnsupportedCategory_FailsValidation()
        {
            var toy = new Toy
            {
                Name = "Toy Car",
                LocationCode = "A01",
                Category = "Language"
            };

            var results = ValidateModel(toy);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Toy.Category)));
        }
    }
}
