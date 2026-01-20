using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebAPI.Validations;

namespace WebApiTests.UnitTests.Validations
{   
    [TestClass]
    public class FirstLetterUppercaseAttributeTest
    {
        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow(null)]
        [DataRow("Felipe")]
        public void Is_Valid_ReturnSuccessful_IfValueDontHaveFirstLetterLowercase(string value) 
        { 
            //prepareite
            var firstLetterUppercaseAttribute = new FirstLetterUppercaseAttribute();
            var validationContext = new ValidationContext(new object());

            // Test
            var result = firstLetterUppercaseAttribute.GetValidationResult(value, validationContext);

            // Verification

            Assert.AreEqual(expected: ValidationResult.Success, actual: result);

        }
        [TestMethod]
        [DataRow("felipe")]
        public void Is_Valid_ReturnError_IfValueHaveFirstLetterLowercase(string value)
        {
            //prepareite
            var firstLetterUppercaseAttribute = new FirstLetterUppercaseAttribute();
            var validationContext = new ValidationContext(new object());

            // Test
            var result = firstLetterUppercaseAttribute.GetValidationResult(value, validationContext);

            // Verification

            Assert.AreEqual(expected: "La primera letra debe ser mayúscula", actual: result!.ErrorMessage);

        }
    }
}
