using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PromBot.Commands;

namespace PromBot.CommandModules.Dice.Commands
{
    internal class DiceCommand : ChannelCommand
    {
        public DiceCommand(ChannelModule module) : base(module)
        { }

        internal override void Init(CommandGroupBuilder cgb)
        {
            cgb.CreateCommand(Module.Prefix + "rolldice")
                .Description("Roll a Dice")
                .Parameter("type", ParameterType.Required)
                .Parameter("quantity", ParameterType.Optional)
                .Do(DoRoll());
        }

        private Func<CommandEventArgs, Task> DoRoll() =>
           async (e) => {
               if (!e.IsAdmin) return;
               var dB = new Models.DiceBag();

               if (!string.IsNullOrWhiteSpace(e.GetArg(1)))
               {
                   var diceResults = new List<int>();
                   uint quantInt;
                   if (!uint.TryParse(e.GetArg(1), out quantInt)) return;
                   switch (e.GetArg(0))
                   {
                       case "d2":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D2, quantInt);
                           break;
                       case "d4":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D4, quantInt);
                           break;
                       case "d6":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D6, quantInt);
                           break;
                       case "d8":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D8, quantInt);
                           break;
                       case "d10":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D10, quantInt);
                           break;
                       case "d12":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D12, quantInt);
                           break;
                       case "d20":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D20, quantInt);
                           break;
                       case "d30":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D30, quantInt);
                           break;
                       case "d50":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D50, quantInt);
                           break;
                       case "d60":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D60, quantInt);
                           break;
                       case "d100":
                           diceResults = dB.RollQuantity(Models.DiceBag.Dice.D100, quantInt);
                           break;
                       default:
                           return;
                   }
                   await e.Client.SendMessage($".me says @{e.Message.ChatMessage.DisplayName} rolled {quantInt} {e.GetArg(0)} Dice. The Total was: {diceResults.Sum()}").ConfigureAwait(false);
               }
               else
               {
                   int diceResult;
                   switch (e.GetArg(0))
                   {
                       case "d2":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D2);
                           break;
                       case "d4":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D4);
                           break;
                       case "d6":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D6);
                           break;
                       case "d8":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D8);
                           break;
                       case "d10":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D10);
                           break;
                       case "d12":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D12);
                           break;
                       case "d20":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D20);
                           break;
                       case "d30":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D30);
                           break;
                       case "d50":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D50);
                           break;
                       case "d60":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D60);
                           break;
                       case "d100":
                           diceResult = dB.Roll(Models.DiceBag.Dice.D100);
                           break;
                       default:
                           return;
                   }
                   await e.Client.SendMessage($".me says @{e.Message.ChatMessage.DisplayName} rolled a single {e.GetArg(0)} Dice. The Total was: {diceResult}").ConfigureAwait(false);
               }
           };
    }
}