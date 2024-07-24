using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using TaskManager.Helpers;
using NuGet.Common;
using Microsoft.VisualBasic;
using Microsoft.Identity.Client;
namespace TaskManager.Service{

    public class ValidationHelper{

        public static (bool Result,string Message) ValidDates(DateOnly dateCreated, DateOnly dueDate){
            if(!DateChecker(dateCreated)){
                return(false, "Please Enter Date After: 2024/01/01");
            }
            if(!DateChecker(dueDate)){
                return(false, "Please Enter Date After: 2024/01/01");
            }
            if(dateCreated>dueDate){
                return (false, $"Invalid values for dates entered: DueDate ({dueDate}) should be later than DateCreated ({dateCreated}). Please ensure that the DueDate is set to a date that is after the DateCreated.");
            }
            else{
                return (true, "Valid date created and due date entered");
            }
        }

        public static bool ValidInput(string input){
            if(string.IsNullOrEmpty(input)){
                return false;
            }
            return true;
        }

        private static bool DateChecker(DateOnly date){
         
            DateOnly initialDate = new DateOnly(2024, 1, 1);
            return date>initialDate;
        }

    }
}