using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using myChatRoomZ.Data.Models;
using myChatRoomZ.Services;


namespace myChatRoomZ.SignalRHub
{
    public class ChatHub:Hub
    {
        private readonly IChatGroupService _chatGroupService;

        public ChatHub(IChatGroupService chatGroupService)
        {
            _chatGroupService = chatGroupService;
        }
    
        public async Task SendMessage(string name,string text,string channelId, int msgId = 0)
        {
            var message = new ChatMessage
            {
                Id= msgId,
                SenderName = name,
                Text = text,
                SentAt = DateTimeOffset.UtcNow,
                ChannelId = Convert.ToInt32(channelId)
            };


            //Broadcast to all Clients,connected to Specific Channel(Group) 
            //The Name the function,that we're invoking on the Client:"RecieveMessage"
            await Clients.Group(channelId).SendAsync("RecieveMessage", message, channelId);
        }

        public async Task JoinChannel(string name,string channelId)
        {
            //1.When Client Joins the Channel we will first remove him from other Group on the Hub
            string connected_channelId =_chatGroupService.RemoveConnectionfromGroups(Context.ConnectionId);
            if (connected_channelId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, connected_channelId);
                //We will also call function on Client to Remove Chatter and Update the list connected chatters for Specific Channel
                if(connected_channelId != channelId)
                await SendMessage(name, "Leaving the Channel", connected_channelId);
                await Clients.All.SendAsync("LeaveChannel", name, connected_channelId);
                 
            }

            //2.When Client joins the Channel we will add him to appropriate Group on the Hub
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
            _chatGroupService.AddConnectiontoGroup(Context.ConnectionId, channelId);

            //3.Broadcast to all Clients,connected to Specific Channel(Group)
            //await Clients.Group(channelId).SendAsync("JoinChannel", name, channelId);
            //The Name the function,that we're invoking on the Client:"JoinChannel"

            //Updating Clients if somebody have joined specific channel
            //(The list of connected chatters for Specific Channel every Client will be updated)
            await Clients.All.SendAsync("JoinChannel", name, channelId);
            await SendMessage(name, "Joining the Channel", channelId);
        }

        public async Task RemoveMessage(string message_date, string channelId)
        {
            //Broadcast to all Clients,connected to Specific Channel(Group) 
            //The Name the function,that we're invoking on the Client:"RemoveMessage"
            await Clients.Group(channelId).SendAsync("RemoveMessage", message_date, channelId);
        }
    }
}
