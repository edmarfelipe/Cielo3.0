using CieloTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Cielo.Tests
{
    [TestClass()]
    public class CieloApiTests
    {
        private string _nome;
        private string _nomeCartao;
        private string _descricao;
        private CieloApi _api;
        private DateTime _validDate;
        private DateTime _invalidDate;

        [TestInitialize]
        public void ConfigEnvironment()
        {
            ISerializerJSON json = new SerializerJSON();

            _api = new CieloApi(CieloEnvironment.SANDBOX, Merchant.SANDBOX, json);
            _validDate = DateTime.Now.AddYears(1);
            _invalidDate = DateTime.Now.AddYears(-1);

            _nome = "Hugo Alves";
            _nomeCartao = "Hugo de Brito V. R. Alves";
            _descricao = "Teste Cielo";
        }

        [TestMethod()]
        public void Autorizacao()
        {
            decimal value = 150.01M;
            CardBrand brand = CardBrand.Visa;

            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: brand);

            var payment = new Payment(
                amount: value,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;


            Assert.IsTrue(returnTransaction.Payment.CreditCard.GetBrand() == brand, "Erro na bandeira do cart�o");
            Assert.IsTrue(returnTransaction.Payment.CreditCard.ExpirationDate == _validDate.ToString("MM/yyyy"), "Erro na data de vencimento do cart�o");
            Assert.IsTrue(returnTransaction.Payment.GetAmount() == value, "Erro no valor da fatura");
            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Authorized, "Transa��o n�o foi autorizada");
        }

        [TestMethod()]
        public void TransacaoCapturada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 2500,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.PaymentConfirmed, "Transa��o n�o teve pagamento confirmado");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoNaoAutorizadaResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.NotAuthorized,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.02M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Denied, "Transa��o n�o foi negada");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoBloqueadoResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.NotAuthorizedCardBlocked,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.06M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Denied, "Transa��o n�o foi negada");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoCanceladoResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.NotAuthorizedCardCanceled,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.03M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Denied, "Transa��o n�o foi negada");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoExpiradoResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.NotAuthorizedCardExpired,
                holder: _nomeCartao,
                expirationDate: _invalidDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.04M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            try
            {
                var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

                Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Denied, "Transa��o n�o foi negada");
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    if (e is CieloException ex)
                    {
                        Assert.IsTrue(ex.GetCieloErrors().Any(i => i.Code == 126));
                    }
                }
                else
                {
                    throw e;
                }
            }
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoComProblemasResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.NotAuthorizedCardProblems,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.05M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Denied, "Transa��o n�o foi negada");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoTimeOut()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.NotAuthorizedTimeOut,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.06M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsTrue(returnTransaction.Payment.ReturnCode == "99", "Resultado esperado Time Out (C�digo 99).");
        }

        [TestMethod()]
        public void AutorizacaoDepoisCaptura()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.07M,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;
            var captureTransaction = _api.CaptureTransaction(Guid.NewGuid(), result.Payment.PaymentId.Value).Result;

            Assert.IsTrue(captureTransaction.GetStatus() == Status.PaymentConfirmed, "Captura n�o teve pagamento confirmado");
        }

        [TestMethod()]
        public void AutorizacaoDepoisCapturaDepoisCancelaResultadoCancelado()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.08M,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;
            var captureTransaction = _api.CaptureTransaction(Guid.NewGuid(), result.Payment.PaymentId.Value).Result;
            var cancelationTransaction = _api.CancelTransaction(Guid.NewGuid(), result.Payment.PaymentId.Value).Result;

            Assert.IsTrue(cancelationTransaction.GetStatus() == Status.Voided, "Cancelamento n�o teve sucesso");
        }

        [TestMethod()]
        public void AutorizacaoDepoisCapturaParcial()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.25M,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;
            var captureTransaction = _api.CaptureTransaction(Guid.NewGuid(), result.Payment.PaymentId.Value, 25.00M).Result;

            Assert.IsTrue(captureTransaction.GetStatus() == Status.PaymentConfirmed, "Transa��o n�o teve pagamento aprovado");
        }

        [TestMethod()]
        public void AutorizacaoComTokenizacaoDoCartao()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: true);

            var payment = new Payment(
                amount: 157.37M,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsNotNull(result.Payment.CreditCard.CardToken, "N�o foi criado o token");
        }

        [TestMethod()]
        public void TransacaoCapturadaComTokenizacao()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: true);

            var payment = new Payment(
                amount: 150.09M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                creditCard: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsNotNull(result.Payment.CreditCard.CardToken, "N�o foi criado o token");
        }

        [TestMethod()]
        public void TransacaoRecorrenteAgendada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Master,
                saveCard: true);

            var recurrentPayment = new RecurrentPayment(
                interval: Interval.Monthly,
                startDate: DateTime.Now.AddMonths(1),
                endDate: DateTime.Now.AddMonths(7));

            var payment = new Payment(
                amount: 150.01M,
                currency: Currency.BRL,
                installments: 1,
                softDescriptor: _descricao,
                creditCard: creditCard,
                recurrentPayment: recurrentPayment);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsTrue(result.Payment.GetStatus() == Status.Scheduled, "Recorr�ncia n�o foi programada");
        }


        [TestMethod()]
        public void TransacaoRecorrenteAgora()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: false);

            var recurrentPayment = new RecurrentPayment(
                interval: Interval.Monthly,
                endDate: DateTime.Now.AddMonths(6));

            var payment = new Payment(
                amount: 150.02M,
                currency: Currency.BRL,
                installments: 1,
                softDescriptor: _descricao,
                creditCard: creditCard,
                recurrentPayment: recurrentPayment);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

            Assert.IsTrue(result.Payment.GetStatus() == Status.Authorized, "Recorr�ncia n�o foi autorizada");
        }

        [TestMethod()]
        public void TransacaoCancelarRecorrencia()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: false);

            var recurrentPayment = new RecurrentPayment(
                interval: Interval.Monthly,
                startDate: DateTime.Now.AddDays(2),
                endDate: DateTime.Now.AddMonths(6));

            var payment = new Payment(
                amount: 150.03M,
                currency: Currency.BRL,
                installments: 1,
                softDescriptor: _descricao,
                creditCard: creditCard,
                recurrentPayment: recurrentPayment);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            //https://apisandbox.cieloecommerce.cielo.com.br/1/RecurrentPayment/{RecurrentPaymentId}/Deactivate

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;
            var result2 = _api.DeactivateRecurrent(Guid.NewGuid(), result.Payment.RecurrentPayment.RecurrentPaymentId.Value).Result;

            Assert.IsTrue(result2, "Recorr�ncia n�o foi desativada");
        }

        [TestMethod()]
        public void TransacaoReabilitarRecorrencia()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new CreditCard(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: false);

            var recurrentPayment = new RecurrentPayment(
                interval: Interval.Monthly,
                startDate: DateTime.Now.AddDays(2),
                endDate: DateTime.Now.AddMonths(6));

            var payment = new Payment(
                amount: 150.05M,
                currency: Currency.BRL,
                installments: 1,
                softDescriptor: _descricao,
                creditCard: creditCard,
                recurrentPayment: recurrentPayment);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;
            var result2 = _api.ActivateRecurrent(Guid.NewGuid(), result.Payment.RecurrentPayment.RecurrentPaymentId.Value).Result;
            var result3 = _api.DeactivateRecurrent(Guid.NewGuid(), result.Payment.RecurrentPayment.RecurrentPaymentId.Value).Result;

            Assert.IsTrue(result3, "Recorr�ncia n�o foi reativada");
        }

        [TestMethod()]
        public void Boleto()
        {
            /*
                VERIFICAR NA CIELO SE SEU CADASTRO PERMITE FAZER PAGAMENTO POR BOLETO
            */

            decimal value = 162.55M;
            string boletoNumber = "0123456789";

            var customer = new Customer(name: _nome);
            customer.Address = new Address()
            {
                ZipCode = "3100000",
                City = "BH",
                State = "MG",
                Street = "Rua Teste",
                Number = "321",
                Country = "BR"
            };

            var payment = new Payment(value,
                                      PaymentType.Boleto,
                                      Provider.Simulado,
                                      boletoNumber,
                                      "Instructions 123");

            var date = _validDate.AddDays(3);
            payment.SetExpirationDate(date);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            try
            {
                var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

                Assert.IsTrue(returnTransaction.Payment.GetPaymentType() == PaymentType.Boleto, "Erro no tipo de pagamento");
                Assert.IsTrue(!string.IsNullOrEmpty(returnTransaction.Payment.BarCodeNumber), "Erro c�digo de barra");
                Assert.IsTrue(!string.IsNullOrEmpty(returnTransaction.Payment.DigitableLine), "Erro linha digit�vel");
                Assert.IsTrue(!string.IsNullOrEmpty(returnTransaction.Payment.Links[0].Href), "Erro na url para redirecionar para o Boleto");
            }
            catch (Exception ex)
            {
                var e = ex.InnerException as CieloException;
                throw;
            }
        }

        [TestMethod()]
        public void TransferenciaOnline()
        {
            /*
                 VERIFICAR NA CIELO SE SEU CADASTRO PERMITE FAZER PAGAMENTO POR TEF
            */

            decimal value = 162.55M;

            var customer = new Customer(name: _nome);
 
            var payment = new Payment(value,
                                      PaymentType.EletronicTransfer,
                                      Provider.Simulado,
                                      transferReturnUrl: "www.cielo.com.br");

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            try
            {
                var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction).Result;

                Assert.IsTrue(returnTransaction.Payment.GetPaymentType() == PaymentType.EletronicTransfer, "Erro no tipo de pagamento");
                Assert.IsTrue(!string.IsNullOrEmpty(returnTransaction.Payment.Links[0].Href), "Erro na url para redirecionar para transferencia eletronica");
            }
            catch (Exception ex)
            {
                var e = ex.InnerException as CieloException;
                throw;
            }
        }
    }
}