//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import Foundation

// The PollingService is a PoC of a self triggered REST Service via a timer, which collects avionic paramters from a remote server

class PollingService: NSObject, URLSessionDelegate, ObservableObject {
    @Published var connectionState: Bool = false
    @Published var pollingResults: [AvionicParameter] = []
    
    var pollingTimer = Timer()    
    var counter = 0    
    var address = ""
    var socket: URLSession?
	
    // Since we use self signed certificates in our environment, we have to auto accept them.
    func urlSession(_ session: URLSession, didReceive challenge: URLAuthenticationChallenge, completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void) {
        print("!!!!!!!!!!!!!!!!!!!!!UNSECURE / CERT NOT VALIDATED!!!!!!!!!!!!!!!!!!!")
        completionHandler(.useCredential,URLCredential(trust: challenge.protectionSpace.serverTrust!))
    }
    
	// startPolling connects to the remote server and starts a polling Timer with a given deltaT in seconds
    func startPolling(address: String, deltaT: Double) -> Void {
        self.address = address
        self.socket = URLSession(configuration: URLSessionConfiguration.default, delegate: self, delegateQueue: nil)
        
		// if a timer was running we have to invalidate it first
        pollingTimer.invalidate()
        pollingTimer = Timer.scheduledTimer(withTimeInterval: deltaT, repeats: true) { timer in
            self.get(uri: "/api/v1/parameters")
        }
        return
    }    

    func get(uri: String){
        let task = socket?.dataTask(with: URL(string:address+uri)!) {
            (data, response, error) in
            guard error == nil else {
			    // Error handling missing!
                print(error!)
                return
            }
            guard let responseData = data else {
                print("No Data Received")
                return
            }
            do {
			    // could be optimized with better deserialization of the parameter object
                guard let parameters = try JSONSerialization.jsonObject(with: responseData, options: []) as? [String: Any] else {
                    print("ERROR")
                    return
                }
                
                for parameter in parameters["parameters"] as! NSArray {
                    let para = parameter as! NSDictionary
                    DispatchQueue.main.async {
                        let parameterIndex = self.pollingResults.firstIndex(where: {$0.name == para.object(forKey: "k") as! String})
                        if  parameterIndex != nil {
                            self.pollingResults[parameterIndex!].value = para.object(forKey: "v") as! String
                            self.pollingResults[parameterIndex!].state = para.object(forKey: "s") as! String
                            self.pollingResults[parameterIndex!].timestamp = para.object(forKey: "t") as! String
                        } else {
                            self.pollingResults.append(AvionicParameter(id: UUID(), name: para.object(forKey: "k") as! String, value: para.object(forKey: "v") as! String, timestamp: para.object(forKey: "t") as! String, state: para.object(forKey: "s") as! String))
                        }
                    }                    
                }                
            } catch {
			    // Error handling missing!
                print("Error")
                return
            }            
        }
        task?.resume()
    }    
    
    func stopPolling() {
        pollingTimer.invalidate()
    }
}
