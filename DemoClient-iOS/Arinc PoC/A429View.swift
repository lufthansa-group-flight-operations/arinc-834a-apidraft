//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import SwiftUI

// This view visualizes communication of A429 via REST
struct A429View: View {
    
    @State var serverAddress: String = "wss://server:port"
    @State var command: String = ""
    
    @ObservedObject var a429Client = A429Service()
    
    var body: some View {
        HStack {
            VStack(alignment: .leading) {
                Text("A429 over WebSocket").font(.title)
                HStack {
                    TextField("wss://server:port",text: self.$serverAddress).background(Color.white.shadow(radius: 2)).foregroundColor(Color.black)
                    Button(action:{self.a429Client.connect(address: self.serverAddress)}){Text("Start")}
                }
                HStack {
                    TextField("Command",text: self.$command).background(Color.white.shadow(radius: 2)).foregroundColor(Color.black)
                    Button(action:{self.a429Client.send(data: self.command)}){Text("Send")}
                }
                Text(a429Client.history).multilineTextAlignment(.leading).lineLimit(nil)
            }
        }
    }
}

struct A429View_Previews: PreviewProvider {
    static var previews: some View {
        A429View()
    }
}
