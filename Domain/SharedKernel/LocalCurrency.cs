using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.SharedKernel;

[JsonConverter(typeof(StringEnumConverter))]
public enum LocalCurrency
{
    /// <summary>
    /// La crypto por default de QvaPay
    /// </summary>
    USDT,

    /// <summary>
    /// Los mismísimos bitcoin.
    /// </summary>
    BTC,
}     