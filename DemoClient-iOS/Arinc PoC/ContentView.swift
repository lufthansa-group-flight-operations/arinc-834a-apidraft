//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import SwiftUI

// The ContentView takes the global activeView as an input to switch between different views
struct ContentView: View {
    
    @EnvironmentObject var activeView: ViewRouter
    
    var body: some View {
        GeometryReader{ geometry in
            VStack{
                HStack{
                    Image("crane").resizable().frame(width: 30, height: 30)
                    Text("LHG  - A834-A RESTful PoC").foregroundColor(.white)
                }.frame(width: geometry.size.width, height:85).background(Color.init(red: 0.027, green: 0.114, blue: 0.286))
                
                if self.activeView.activeView == "home" {
                    Text("")
                    VStack(alignment: .leading) {
                        Text("About this App").font(.title)
                        Text("\n\nThis is a Proof-of-Concept (PoC) for the Arinc Comittee. The intention of this application is to showcase different use cases provided via REST & WebSockets. \n\n !!!IMPORTANT!!! This app accepts every certificate for TLS. There is no validation of the certificate.").multilineTextAlignment(.leading).lineLimit(nil)
                    }
                } else if self.activeView.activeView == "polling" {
                    HStack{
                        Spacer()
                        PollingView()
                        Spacer()
                    }
                } else if self.activeView.activeView == "subscription" {
                    HStack{
                        Spacer()
                        SubscriptionView()
                        Spacer()
                    }
                } else if self.activeView.activeView == "file" {
                    HStack{
                        Spacer()
                        FileListView()
                        Spacer()
                    }
                } else if self.activeView.activeView == "a429" {
                    HStack{
                        Spacer()
                        A429View()
                        Spacer()
                    }
                } else if self.activeView.activeView == "video" {
                    Text("Video coming soon...")
                } else {
                    Text("OTHER")
                }
                
                Spacer()
                MenuView().frame(width: geometry.size.width, height:85).background(Color.white.shadow(radius: 2))
            }.edgesIgnoringSafeArea(.all)
        }
    }
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
    }
}
