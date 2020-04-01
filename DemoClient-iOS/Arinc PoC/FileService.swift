//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import Foundation
import PDFKit

// The FileService is a PoC of handling RAW Data from a remote server 
class FileService: NSObject, URLSessionDelegate, ObservableObject {
    
    var socket: URLSession?
    @Published var fileContent: String = ""
    @Published var pdfContent = PDFDocument()
    
    @Published var fileListResult: [FileDetail] = []
	
    // Since we use self signed certificates in our environment, we have to auto accept them.
    func urlSession(_ session: URLSession, didReceive challenge: URLAuthenticationChallenge, completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void) {
        print("!!!!!!!!!!!!!!!!!!!!!UNSECURE / CERT NOT VALIDATED!!!!!!!!!!!!!!!!!!!")
        completionHandler(.useCredential,URLCredential(trust: challenge.protectionSpace.serverTrust!))
    }
    
	// We can get a list of files from the remote server
    func getFileList(address: String) -> Void {
        self.socket = URLSession(configuration: URLSessionConfiguration.default, delegate: self, delegateQueue: nil)
        
        let task = socket?.dataTask(with: URL(string:address)!) {
            (data, response, error) in
            guard error == nil else {
			    // failure handling missing!
                print(error!)
                return
            }
            guard let responseData = data else {
                print("No Data Received")
                return
            }
            do {
                guard let fileList = try JSONSerialization.jsonObject(with: responseData, options: []) as? [String: Any] else {
                    print("ERROR")
                    return
                }
                DispatchQueue.main.async {self.fileListResult=[]}
                for fileListEntry in fileList["files"] as! NSArray {
                    let entry = fileListEntry as! NSDictionary
                    DispatchQueue.main.async {self.fileListResult.append(FileDetail(id: UUID(),name: entry.object(forKey: "name") as! String,size: entry.object(forKey: "size") as! String,lastChange: entry.object(forKey: "last_change") as! String))}
                }                
            } catch {
			    // failure handling missing!
                print("Error")
                return
            }            
        }

        task?.resume()        
        return
    }

	// pulls a given file from a remote server and provides the content as fileContent and pdfContent for later visualization 
    func getFile(uri: String) -> Void {
        self.socket = URLSession(configuration: URLSessionConfiguration.default, delegate: self, delegateQueue: nil)
        
        let task = socket?.dataTask(with: URL(string:uri)!) {            
            (data, response, error) in
            guard error == nil else {
			    // failure handling missing!
                print(error!)
                return
            }
            guard let responseData = data else {
                print("No Data Received")
                return
            }
            self.fileContent = String(bytes: responseData, encoding: .ascii)!
            if uri.hasSuffix("pdf") {
                self.pdfContent = PDFDocument(data: responseData) ?? PDFDocument()
            }
            return            
        }

        task?.resume()
    }
}
