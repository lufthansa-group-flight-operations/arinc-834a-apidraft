//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import Foundation
import UIKit

// The SubscriptionService implements WebSockets for a representation of a subscription
// It receives the avionmic parameters without polling the server and publishes the result

class SubscriptionService : NSObject, URLSessionWebSocketDelegate, ObservableObject {
    
    var socket: URLSessionWebSocketTask!
    @Published var subResults: [AvionicParameter] = []
	
    // Since we use self signed certificates in our environment, we have to auto accept them.
    func urlSession(_ session: URLSession, didReceive challenge: URLAuthenticationChallenge, completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void) {
        print("!!!!!!!!!!!!!!!!!!!!!UNSECURE / CERT NOT VALIDATED!!!!!!!!!!!!!!!!!!!")
        completionHandler(.useCredential,URLCredential(trust: challenge.protectionSpace.serverTrust!))
    }
    
	// The connection to the server which auto resumes the task and starts the receive loop
    func connect(address: String){
        socket = URLSession(configuration: .default, delegate: self, delegateQueue: nil).webSocketTask(with: URL(string: address + "/api/v1/parameters")!)
        socket.resume()
        receive()
    }
    
	// Receives the data which the server provides
    func receive() {
		// the result of a receive can be an error or a success 
        socket.receive { result in
            switch result {
            case .failure(let error):
			    // failure handling missing!
                print(error)
            case .success(let message):
			    // if the receive was a success it can be raw data or a string
			    // in this PoC we only expect a JSON String
                switch message {
                case .data(let data):
                    print("Data \(data)")                    
                    
                case .string(let data):
                    do {
					    // could be optimized with a better serialization
                        let inputData = data.data(using: .utf8)
                        let parameters = try JSONSerialization.jsonObject(with: inputData!, options: []) as? [String: Any]
                        for parameter in parameters!["parameters"] as! NSArray {
                            let para = parameter as! NSDictionary
                            DispatchQueue.main.async {
                                let parameterIndex = self.subResults.firstIndex(where: {$0.name == para.object(forKey: "k") as! String})
                                if  parameterIndex != nil {                                    
                                    self.subResults[parameterIndex!].value = para.object(forKey: "v") as! String
                                    self.subResults[parameterIndex!].state = para.object(forKey: "s") as! String
                                    self.subResults[parameterIndex!].timestamp = para.object(forKey: "t") as! String                                    
                                } else {
                                    self.subResults.append(AvionicParameter(id: UUID(),name: para.object(forKey: "k") as! String,value: para.object(forKey: "v") as! String,timestamp: para.object(forKey: "t") as! String,state: para.object(forKey: "s") as! String))
                                }                                
                            }
                        }
                    } catch {
					    // failure handling missing!
                        print("Error")
                    }
                    
                @unknown default:
                    fatalError()
                }

                self.receive()
            }
        }
    }

    // We could send a message async to the receive to update subscriptions
    func send(data: String) {
        socket.send(URLSessionWebSocketTask.Message.string(data)) { error in
            print("error")
        }
    }
	
    // We could send a raw data async to the receive
    func send(data: Data) {
        socket.send(URLSessionWebSocketTask.Message.data(data)) { error in
            print("error")
        }
    }
    
    func disconnect() {
        socket.cancel()
    }
}
