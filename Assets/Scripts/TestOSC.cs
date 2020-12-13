using System.Collections;
using System.Collections.Generic;
using extOSC;
using UnityEngine;

public class TestOSC : MonoBehaviour
{
    private OSCTransmitter _transmitter;
    
    // Start is called before the first frame update
    void Start()
    {
        _transmitter = GetComponent<OSCTransmitter>();
    }

    // Update is called once per frame
    void Update()
    {
        // Create message
        var message = new OSCMessage("/message/address");

        // Populate values.
        message.AddValue(OSCValue.String("Hello, world!"));

        // Send message
        _transmitter.Send(message);
        Debug.Log("Message sent !");
        
        
    }
}
