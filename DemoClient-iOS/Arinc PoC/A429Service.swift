//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import Foundation

// The A429Service connects to a remote server via WebSocket 
// It can send commands and receive them
// All commands are saved in a history for later visualization
class A429Service : NSObject, URLSessionWebSocketDelegate, ObservableObject {
    
    var socket: URLSessionWebSocketTask!
    var history: String = ""
    // Since we use self signed certificates in our environment, we have to auto accept them.
    func urlSession(_ session: URLSession, didReceive challenge: URLAuthenticationChallenge, completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void) {
        print("!!!!!!!!!!!!!!!!!!!!!UNSECURE / CERT NOT VALIDATED!!!!!!!!!!!!!!!!!!!")
        completionHandler(.useCredential,URLCredential(trust: challenge.protectionSpace.serverTrust!))
    }
    
    func connect(address: String) {
        socket = URLSession(configuration: .default, delegate: self, delegateQueue: nil).webSocketTask(with: URL(string:address)!)
        socket.resume()
        receive()        
    }
    
    func receive() {
        socket.receive { result in
            switch result {
            case .failure(let error):
			    // failure handling missing!
                print(error)
            case .success(let message):
                switch message {
                case .data(let data):
                    print("Data \(data)")
                    self.history = self.history + "\n RX: " + String(bytes: data, encoding: .utf8)!;
                                    
                case .string(let data):
                    print("String \(data)")
                    self.history = self.history + "\n RX: " + data;
                    
                @unknown default:
                    fatalError()
                }

                print("OK")
                self.receive()
            }
        }
    }
        
    func send(data: String) {
        self.history = self.history + "\n TX: " + data;
        socket.send(URLSessionWebSocketTask.Message.string(data)) { error in
            print("error")
        }
    }
    
    func send(data: Data) {
        self.history = self.history + "\n TX: " + String(bytes: data, encoding: .utf8)!;
        socket.send(URLSessionWebSocketTask.Message.data(data)) { error in
            print("error")
        }
    }
        
}
