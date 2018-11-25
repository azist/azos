/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.Conf;
using Azos.Collections;
using Azos.Financial;
using Azos.Instrumentation;

namespace Azos.Web.Pay
{
  /// <summary>
  /// Represents a web terminal for pay systems that tokenize sensitive CC data via a call to provider
  /// so that actual CC numbers never touch our servers in a plain form, instead tokens/nonces are supplied
  /// back by the provider tokenizer. This is needed for PCI compliance.
  /// </summary>
  public interface IPayWebTerminal
  {
    /// <summary>
    /// References pay system that this terminal services
    /// </summary>
    IPaySystem PaySystem { get; }

    /// <summary>
    /// Returns client script body that initializes WAVE.Pay by calling WAVE.Pay.init(...) to perform operation
    /// against the provider.
    /// </summary>
    object GetPayInit();
  }


  /// <summary>
  /// Describes an entity that can perform pay functions (i.e. charge, transfer)
  /// </summary>
  public interface IPaySystem: INamed
  {
    /// <summary>
    /// Returns a pay terminal is this payment provider supports it or null
    /// </summary>
    IPayWebTerminal WebTerminal { get; }

    /// <summary>
    /// Config node of params used inside <see cref="StartSession(ConnectionParameters, IPaySessionContext)"/> method
    /// if PayConnectionParameters parameter is null
    /// </summary>
    IConfigSectionNode DefaultSessionConnectParamsCfg { get; set; }

    /// <summary>
    /// Processing fee types, such as: included in amount and surcharged.
    /// </summary>
    ProcessingFeeKind ChargeFeeKind { get; }

    /// <summary>
    /// Processing fee types, such as: included in amount and surcharged.
    /// </summary>
    ProcessingFeeKind TransferFeeKind { get; }

    /// <summary>
    /// Returns currency ISOs that are supported by this isntance. The processing of charges/transafers may be done
    /// only in these currencies
    /// </summary>
    IEnumerable<string> SupportedCurrencies{ get; }

    /// <summary>
    /// Returns true if this system supports transaction type in the specified currency (optional)
    /// </summary>
    bool IsTransactionTypeSupported(TransactionType type, string currencyISO = null);

    /// <summary>
    /// Starts new pay session of system-specific type.
    /// If cParams parameter is null <see cref="DefaultSessionConnectParamsCfg"/> is used
    /// </summary>
    PaySession StartSession(ConnectionParameters cParams = null, IPaySessionContext context = null);
  }

  /// <summary>
  /// Pay system with fee
  /// </summary>
  public interface IPaySystemWithFee
  {
    /// <summary>
    /// Get Transaction Flat Fee by CurrencyISO and TransactionType
    /// </summary>
    Amount GetTransactionFlatFee(string currencyISO, TransactionType type);

    /// <summary>
    /// Get Transaction Pct Fee by CurrencyISO and TransactionType
    /// </summary>
    decimal GetTransactionPctFee(string currencyISO, TransactionType type);
  }

  /// <summary>
  /// Describes an entity that can perform pay functions with several useful interfaces in Azos style
  /// </summary>
  public interface IPaySystemImplementation: IPaySystem, IConfigurable, IInstrumentable
  {
  }
}
