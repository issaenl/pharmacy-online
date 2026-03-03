using pharmacyBackend.Enums;
using pharmacyBackend.Models;

namespace pharmacyBackend.Services
{
    public class EmailGenerator : IEmailGenerator
    {
        public (string Subject, string HtmlBody) GenerateStatusEmail(User user, Order order, OrderStatus status, Cart? cart = null, string? emailItemsList = null, decimal? totalPrice = null)
        {
            string subject = "";
            string statusText = "";
            string messageBody = "";
            string additionalDetails = "";

            switch (status)
            {
                case OrderStatus.Pending:
                    subject = $"Заказ №{order.Id} успешно забронирован!";
                    statusText = "ОЖИДАЕТ СБОРКИ";
                    messageBody = "Ваш заказ успешно сформирован и передан фармацевту для сборки.";

                    if (cart != null && cart.Pharmacy != null)
                    {
                        additionalDetails = $@"
                            <br>
                            Пункт выдачи: <b>{cart.Pharmacy.Name}</b><br>
                            Адрес: {cart.Pharmacy.Address}, {cart.Pharmacy.District}
                            <br><br>
                            <b>Состав заказа:</b>
                            <ul style='margin-top: 5px; margin-bottom: 15px;'>{emailItemsList}</ul>
                            К оплате при получении: <b>{totalPrice:F2} р.</b>
                        ";
                    }
                    break;

                case OrderStatus.Ready:
                    subject = $"Ваш заказ №{order.Id} готов к выдаче!";
                    statusText = "ГОТОВ К ВЫДАЧЕ";
                    messageBody = "Ура! Ваш заказ собран и ожидает вас в аптеке. Пожалуйста, заберите его в течение 48 часов.";
                    break;

                case OrderStatus.Completed:
                    subject = $"Заказ №{order.Id} успешно получен";
                    statusText = "ВЫПОЛНЕН";
                    messageBody = "Вы успешно забрали свой заказ. Спасибо, что выбираете УниМед! Будем рады видеть вас снова.";
                    break;

                case OrderStatus.Cancelled:
                    subject = $"Заказ №{order.Id} отменен";
                    statusText = "ОТМЕНЕН";
                    messageBody = "Ваш заказ был отменен.";
                    break;

                case OrderStatus.Expired:
                    subject = $"Истек срок хранения заказа №{order.Id}";
                    statusText = "ИСТЕК СРОК ХРАНЕНИЯ";
                    messageBody = "Срок хранения вашего заказа истек, и он был расформирован.";
                    break;

                default:
                    return ("", "");
            }

            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>
                    Здравствуйте, {user.FirstName}!<br><br>
                    
                    Информация о вашем заказе <b>№{order.Id}</b> обновилась.<br><br>
                    
                    Текущий статус: <b>{statusText}</b><br><br>
                    
                    {messageBody}
                    
                    {additionalDetails}
                </div>
            ";

            return (subject, htmlBody);
        }
    }
}