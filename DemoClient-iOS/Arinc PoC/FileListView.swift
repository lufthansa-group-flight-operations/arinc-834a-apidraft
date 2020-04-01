//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import SwiftUI

// This view shows a file list from a remote server and provides FileDetailViews for every file in list
struct FileListView: View {
    
    @ObservedObject var fileService = FileService()
    @State private var serverAddress: String = "https://server:port"

    var body: some View {
        NavigationView {
            VStack(alignment: .leading) {
                Text("File Storage").font(.title)            
            
                HStack {
                    TextField("https://server:port",text: self.$serverAddress).background(Color.white.shadow(radius: 2)).foregroundColor(Color.black)
                    Button(action:{self.fileService.getFileList(address: self.serverAddress+"/api/v1/files")}){Text("Start")}                    
                }            
            
                List(self.fileService.fileListResult) { fileDetail in
                    NavigationLink(destination: FileDetailView(serverAddress: self.serverAddress,fileName:fileDetail.name)) {
                        HStack {
                            Text(fileDetail.name)
                            Spacer()
                            Text(fileDetail.size)
                            Spacer()
                            Text(fileDetail.lastChange)
                        }
                    }
                }
            }.navigationBarTitle("").navigationBarHidden(true)
        }
    }
}

struct FileView_Previews: PreviewProvider {
    static var previews: some View {
        FileListView()
    }
}
