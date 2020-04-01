//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import SwiftUI

// A simple MenuBar which controls the global activeView to control the ContentView 
struct MenuView: View {    
    
    @EnvironmentObject var activeView: ViewRouter
    
    var body: some View {
        GeometryReader{ geometry in
            HStack {
                Spacer()
                VStack {
                    if self.activeView.activeView == "polling" {
                        Image(systemName: "arrow.2.circlepath").resizable().aspectRatio(contentMode: .fit).foregroundColor(.blue)
                    } else {
                        Image(systemName: "arrow.2.circlepath").resizable().aspectRatio(contentMode: .fit).foregroundColor(.black)
                    }
                    Text("Polling").padding(.bottom,10).foregroundColor(Color.black)
                }.frame(width: geometry.size.width/6,height: 75).padding(0).onTapGesture {
                    self.activeView.activeView="polling"
                }
                VStack {
                    if self.activeView.activeView == "subscription" {
                        Image(systemName: "arrow.clockwise").resizable().aspectRatio(contentMode: .fit).foregroundColor(.blue)
                    } else {
                        Image(systemName: "arrow.clockwise").resizable().aspectRatio(contentMode: .fit).foregroundColor(.black)
                    }
                    Text("Subscription").padding(.bottom,10).foregroundColor(Color.black)
                }.frame(width: geometry.size.width/6,height: 75).onTapGesture {
                    self.activeView.activeView="subscription"
                }
                VStack {
                    if self.activeView.activeView == "file" {
                        Image(systemName: "folder").resizable().aspectRatio(contentMode: .fit).foregroundColor(.blue)
                    } else {
                        Image(systemName: "folder").resizable().aspectRatio(contentMode: .fit).foregroundColor(.black)
                    }
                    Text("File").padding(.bottom,10).foregroundColor(Color.black)
                }.frame(width: geometry.size.width/6,height: 75).onTapGesture {
                    self.activeView.activeView="file"
                }
                VStack {
                    if self.activeView.activeView == "a429" {
                        Image(systemName: "airplane").resizable().aspectRatio(contentMode: .fit).foregroundColor(.blue)
                    } else {
                        Image(systemName: "airplane").resizable().aspectRatio(contentMode: .fit).foregroundColor(.black)
                    }
                    Text("A429").padding(.bottom,10).foregroundColor(Color.black)
                }.frame(width: geometry.size.width/6,height: 75).onTapGesture {
                    self.activeView.activeView="a429"
                }
                VStack {
                    if self.activeView.activeView == "video" {
                        Image(systemName: "video").resizable().aspectRatio(contentMode: .fit).foregroundColor(.blue)
                    } else {
                        Image(systemName: "video").resizable().aspectRatio(contentMode: .fit).foregroundColor(.black)
                    }
                    Text("Video").padding(.bottom,10).foregroundColor(Color.black)
                }.frame(width: geometry.size.width/6,height: 75).onTapGesture {
                    self.activeView.activeView="video"
                }
                Spacer()
            }.padding(.top,20)
        }
    }
}

struct MenuView_Previews: PreviewProvider {
    static var previews: some View {
        MenuView()
    }
}
