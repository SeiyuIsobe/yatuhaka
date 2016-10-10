﻿/*
 *  Copyright (c) 2016 IBM Corporation and other Contributors.
 *
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html 
 *
 * Contributors:
 * 	 kaberi Singh - Initial Contribution
 *   Hari hara prasad Viswanathan  - updations
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace IBMWatsonIoTP
{
    /// <summary>
    ///     A client, used by device, that handles connections with the IBM Internet of Things Foundation. <br>
    ///     This is a derived class from AbstractClient and can be used by embedded devices to handle connections with IBM Internet of Things Foundation.
    /// </summary>
    public class DeviceClient : AbstractClient
    {
        static string _orgId ="";
        static string _deviceType ="";
        static string _deviceID ="";
        static string _authmethod ="";
        static string _authtoken ="";
        
        public DeviceClient(string orgId, string deviceType, string deviceID, string authmethod, string authtoken)
            : base(orgId, "d" + CLIENT_ID_DELIMITER + orgId + CLIENT_ID_DELIMITER + deviceType + CLIENT_ID_DELIMITER + deviceID, "use-token-auth", authtoken)
        {

        }

        public DeviceClient(string deviceType, string deviceID)
            : base("quickstart", "d" + CLIENT_ID_DELIMITER + "quickstart" + CLIENT_ID_DELIMITER + deviceType + CLIENT_ID_DELIMITER + deviceID, null, null)
        {

        }
        //public DeviceClient(string filePath) :
        //	base(parseFromFile(filePath), "d" + CLIENT_ID_DELIMITER + _orgId + CLIENT_ID_DELIMITER + _deviceType + CLIENT_ID_DELIMITER + _deviceID, "use-token-auth", _authtoken)
        	
        //{
        	
        	
        //}
        //private static string parseFromFile(string filePath)
        //{
        //	Dictionary<string,string> data = parseFile(filePath,"## Device Registration detail");
        //	if(	!data.TryGetValue("Organization-ID",out _orgId)||
        //		!data.TryGetValue("Device-Type",out _deviceType)||
        //		!data.TryGetValue("Device-ID",out _deviceID)||
        //		!data.TryGetValue("Authentication-Method",out _authmethod)||
        //		!data.TryGetValue("Authentication-Token",out _authtoken) )
        //	{
        //		throw new Exception("Invalid property file");
        //	}
        //	return _orgId;
        //}
        [Obsolete]
        private void MqttMsgReceived(MqttMsgPublishEventArgs e)
        {
        }

        /// <summary>
        ///     Publish event to the IBM Internet of Things Foundation. <br>
        /// </summary>
        /// <param name="evt">
        ///      object of String which denotes event </param>
        /// <param name="format">
        ///      object of String which denotes format </param>
        /// <param name="msg">
        ///      object of String which denotes message </param>
        /// <param name="qosLevel">
        ///     Quality of Service, in int - can have values 0,1,2</param>
        public bool publishEvent(String evt, String format, String msg, byte qosLevel)
        {
        	try{
        		if (!isConnected()) {
					return false;
				}
	           	mqttClient.MqttMsgPublished += client_MqttMsgPublished;
	            string topic = "iot-2/evt/" + evt + "/fmt/" + format;
	            mqttClient.Publish(topic, Encoding.UTF8.GetBytes(msg), qosLevel, false);
	            return true;
        	}
        	catch(Exception e)
        	{
				return false;
        		
        	}
        	
        }

        /// <summary>
        ///     Publish event to the IBM Internet of Things Foundation. <br>
        /// </summary>
        /// <param name="evt">
        ///      object of String which denotes event </param>
        /// <param name="format">
        ///      object of String which denotes format </param>
        /// <param name="msg">
        ///      object of String which denotes message</param> 
        public bool publishEvent(String evt, String format, String msg)
        {
            return publishEvent(evt, format, msg, 0);
        }

        /// <summary>
        ///     Subscribe command to the IBM Internet of Things Foundation. <br>
        /// </summary>
        /// <param name="cmd">
        ///      object of String which denotes command </param>
        /// <param name="format">
        ///      object of String which denotes format </param>
        /// <param name="qosLevel">
        ///     Quality of Service, in int - can have values 0,1,2</param>
        public void subscribeCommand(String cmd, String format, byte qosLevel)
        {
        	try{
	        	string topic = "iot-2/cmd/" + cmd + "/fmt/" + format;
	            string[] topics = { topic };
	            byte[] qos = { qosLevel };
	            mqttClient.Subscribe(topics, qos);
	
	            mqttClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
	
        	}
        	catch(Exception e)
        	{
        	}
            
        }

        /// <summary>
        ///     Publish command to the IBM Internet of Things Foundation. <br>
        /// </summary>
        [Obsolete]
        void client_EventPublished(Object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
        }
		[Obsolete]
        void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
        }
		[Obsolete]
        void client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
        }

        public void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
        	try
        	{
	            // handle message received
	            string result = System.Text.Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
	            
	            string topic = e.Topic;
	            string[] tokens = topic.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
	            
	            this.commandCallback(tokens[2], tokens[4], result);
            }
        	catch(Exception ex)
        	{
        	}
        }

        public delegate void processCommand(string cmdName, string format, string data);

        public event processCommand commandCallback;
    
    }
}
