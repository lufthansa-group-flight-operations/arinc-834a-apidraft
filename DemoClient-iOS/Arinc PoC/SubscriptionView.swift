//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import SwiftUI

// The view visualizes the avionic paramters which are provided by the SubscriptionService via WebSocket
struct SubscriptionView: View {
    @ObservedObject var subscriptionService = SubscriptionService()    
    @State private var serverAdress: String = "wss://server:port"

    var body: some View {
        VStack(alignment: HorizontalAlignment.leading) {
            VStack {
                Text("WebSocket Subscription").font(.title)
            }            
            HStack {
                TextField("wss://server:port",text: self.$serverAdress).background(Color.white.shadow(radius: 2)).foregroundColor(Color.black)
                Text("/api/v1/parameters")
                Button(action:{self.subscriptionService.connect(address: self.serverAdress)}){Text("Start")}
                Button(action:{self.subscriptionService.disconnect()}){Text("Stop")}                
            }
            List(self.subscriptionService.subResults) { parameter in
                AvionicParameterView(parameter: parameter)
            }
        }
    }
}

struct SubscriptionView_Previews: PreviewProvider {
    static var previews: some View {
        SubscriptionView()
    }
}
