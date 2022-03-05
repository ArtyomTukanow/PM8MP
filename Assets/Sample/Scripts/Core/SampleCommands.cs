using System;
using System.Collections.Generic;
using PM8MP.Command;
using PM8MPSample.Command;

namespace PM8MPSample.Core
{
    public class SampleCommands
    {
        public static Dictionary<byte, Type> Commands = new Dictionary<byte, Type>
        {
            {CmdMove.TYPE                       , typeof(CmdMove)               },    //1
            {CmdEatFood.TYPE                    , typeof(CmdEatFood)            },    //4

            {ServerCmdPong.TYPE                 , typeof(ServerCmdPong)},             //247
            {ServerCmdPing.TYPE                 , typeof(ServerCmdPing)},             //248
            {SpecialCmdSendMyUid.TYPE           , typeof(SpecialCmdSendMyUid)},       //250
            {SpecialCmdSendRoomName.TYPE        , typeof(SpecialCmdSendRoomName)},    //251
            {ServerCmdSetMyPlayerId.TYPE        , typeof(ServerCmdSetMyPlayerId)},    //252
            {ServerCmdPlayerLeft.TYPE           , typeof(ServerCmdPlayerLeft)},       //253
        };
    }
}