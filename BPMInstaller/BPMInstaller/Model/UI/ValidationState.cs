using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BPMInstaller.UI.Desktop.Model.UI
{
    public class ValidationState: BaseUIModel
    {
        private SolidColorBrush applicationValidationStateColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush databaseValidationStateColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush redisValidationStateColor = new SolidColorBrush(Colors.White);

        private string? applicationValidationResult = null;
        private string? databaseValidationResult = null;
        private string? redisValidationResult = null;

        public SolidColorBrush ApplicationValidationStateColor 
        { 
            get => applicationValidationStateColor;
            set => Set(ref applicationValidationStateColor, value);
        }

        public SolidColorBrush DatabaseValidationStateColor
        {
            get => databaseValidationStateColor;
            set => Set(ref databaseValidationStateColor, value);
        }

        public SolidColorBrush RedisValidationStateColor
        {
            get => redisValidationStateColor;
            set => Set(ref redisValidationStateColor, value);
        }

        public string? ApplicationValidationResult
        {
            get => applicationValidationResult;
            set => Set(ref applicationValidationResult, value);
        }

        public string? DatabaseValidationResult
        {
            get => databaseValidationResult;
            set => Set(ref databaseValidationResult, value);
        }

        public string? RedisValidationResult
        {
            get => redisValidationResult;
            set => Set(ref redisValidationResult, value);
        }

        public void StartValidation(ValidationOperation operation)
        {
            UpdateValidationState(operation, new SolidColorBrush(Colors.Yellow), string.Empty);
        }

        public void HandleValidationResult(ValidationOperation operation, string validationMessage)
        {
            var color = new SolidColorBrush(string.IsNullOrEmpty(validationMessage) ? Colors.Green : Colors.Red);
            UpdateValidationState(operation, color, validationMessage);
        }

        private void UpdateValidationState(ValidationOperation operation, SolidColorBrush color, string validationMessage)
        {
            switch (operation)
            {
                case ValidationOperation.Application:
                    ApplicationValidationStateColor = color;
                    ApplicationValidationResult = validationMessage;
                    break;
                case ValidationOperation.Database:
                    DatabaseValidationStateColor = color;
                    DatabaseValidationResult = validationMessage;
                    break;
                case ValidationOperation.Redis:
                    RedisValidationStateColor = color;
                    RedisValidationResult = validationMessage;
                    break;
                default:
                    throw new NotImplementedException(operation.ToString());
            }
        }

        public enum ValidationOperation
        {
            Application, 
            Database,
            Redis
        }
    }
}
