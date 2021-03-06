﻿#region License

// Copyright (c) 2005-2014, CellAO Team
// 
// 
// All rights reserved.
// 
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
// 
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of the CellAO Team nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

#endregion

namespace ZoneEngine.Core.MessageHandlers
{
    #region Usings ...

    using CellAO.Core.Components;
    using CellAO.Core.Entities;
    using CellAO.Core.Network;
    using CellAO.Enums;

    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

    using ZoneEngine.Core.InternalMessages;

    #endregion

    /// <summary>
    /// </summary>
    [MessageHandler(MessageHandlerDirection.All)]
    public class FollowTargetMessageHandler : BaseMessageHandler<FollowTargetMessage, FollowTargetMessageHandler>
    {
        /// <summary>
        /// </summary>
        public FollowTargetMessageHandler()
        {
            this.UpdateCharacterStatsOnReceive = true;
        }

        #region Inbound

        /// <summary>
        /// </summary>
        /// <param name="followTargetMessage">
        /// </param>
        /// <param name="client">
        /// </param>
        protected override void Read(FollowTargetMessage followTargetMessage, IZoneClient client)
        {
            // REFACT can we use the base class methods here ??

            // var followTargetMessage = (FollowTargetMessage)message.Body;

            var announce = new FollowTargetMessage { Identity = client.Controller.Character.Identity, Unknown = 0 };
            var followinfo = followTargetMessage.Info as FollowTargetInfo;
            if (followinfo != null)
            {
                announce.Info = new FollowTargetInfo()
                                {
                                    MoveType = 0,
                                    Target = followinfo.Target,
                                    Dummy = 0x40,
                                    Dummy1 = 0x20000000
                                };
            }

            client.Controller.Character.Playfield.Publish(new IMSendAOtomationMessageToPlayfield { Body = announce });
        }

        public void Send(ICharacter character, Vector3 stopPosition)
        {
            this.SendToPlayfield(character, this.FillerFullStopAt(character, stopPosition));
        }

        private MessageDataFiller FillerFullStopAt(ICharacter character, Vector3 stopPosition)
        {
            return x =>
            {
                x.Identity = character.Identity;
                x.Info = new FollowTargetInfo()
                         {
                             Target = Identity.None,
                             X = stopPosition.X,
                             Y = stopPosition.Y,
                             Z = stopPosition.Z,
                             Dummy = 0,
                             Dummy1 = 0,
                             MoveType = 21 // Magic number FULL STOP
                         };
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="character">
        /// </param>
        /// <param name="toFollow">
        /// </param>
        public void Send(ICharacter character, Identity toFollow)
        {
            this.SendToPlayfield(character, this.FillerFollowTarget(character, toFollow));
        }

        /// <summary>
        /// </summary>
        /// <param name="character">
        /// </param>
        /// <param name="start">
        /// </param>
        /// <param name="end">
        /// </param>
        public void Send(ICharacter character, Vector3 start, Vector3 end)
        {
            this.SendToPlayfield(character, this.FillerCoordinates(character, start, end));
        }

        /// <summary>
        /// </summary>
        /// <param name="character">
        /// </param>
        /// <param name="toFollow">
        /// </param>
        /// <returns>
        /// </returns>
        private MessageDataFiller FillerFollowTarget(ICharacter character, Identity toFollow)
        {
            return x =>
            {
                x.Identity = character.Identity;
                x.Info = new FollowTargetInfo() { Target = toFollow, Dummy1 = 0x20000000 };
                x.Unknown = 0;
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="character">
        /// </param>
        /// <param name="start">
        /// </param>
        /// <param name="end">
        /// </param>
        /// <returns>
        /// </returns>
        private MessageDataFiller FillerCoordinates(ICharacter character, Vector3 start, Vector3 end)
        {
            return x =>
            {
                x.Identity = character.Identity;
                byte movetype = 0;
                switch (character.MoveMode)
                {
                    case MoveModes.Crawl:
                        movetype = 27;
                        break;
                    case MoveModes.Run:
                        movetype = 25;
                        break;
                    case MoveModes.Walk:
                    default:
                        movetype = 24;
                        break;
                }
                x.Info = new FollowCoordinateInfo()
                         {
                             CurrentCoordinates = start,
                             EndCoordinates = end,
                             CoordinateCount = 2,
                             MoveMode = movetype,
                             FollowInfoType = 1
                         };
                x.Unknown = 1;
            };
        }

        #endregion
    }
}