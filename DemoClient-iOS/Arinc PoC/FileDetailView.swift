//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import SwiftUI
import PDFKit

// The FileDetailView handles the raw data received via FileService
// It differentiates between raw data and PDF files
// For the purpose to visualize different file formats the TextView and PdfView had to be wrapped into UIViewRepresentable

struct TextView: UIViewRepresentable {
    func updateUIView(_ uiView: UITextView, context: UIViewRepresentableContext<TextView>) {
        uiView.text = text
    }
    
    typealias UIViewType = UITextView
    
    @Binding var text: String
    
    func makeUIView(context: Context) -> UITextView {
        return UITextView()
    }
}

struct PdfView : UIViewRepresentable {
    func updateUIView(_ uiView: PDFView, context: UIViewRepresentableContext<PdfView>) {
        uiView.document = document
        uiView.autoScales = true
    }
    
    typealias UIViewType = PDFView
    
    @Binding var document: PDFDocument
    
    func makeUIView(context: UIViewRepresentableContext<PdfView>) -> PDFView {
        return PDFView()
    }
}

struct FileDetailView: View {
    var serverAddress: String = ""
    var fileName = ""
    @ObservedObject var fileService = FileService()
    
    public init(serverAddress: String, fileName: String) {
        self.fileName = fileName
        print("Get File "+fileName)
        fileService.getFile(uri: serverAddress+"/api/v1/files/storage/"+fileName)
    }
    
	//the body differentiates between any or pdf file
    var body: some View {
        GeometryReader { geometry in
            VStack(alignment: HorizontalAlignment.leading) {
                Text("File Content:").font(.title)
                if self.fileName.hasSuffix("pdf") {
                    PdfView(document: self.$fileService.pdfContent).frame(width: geometry.size.width)
                } else {
                    TextView(text: self.$fileService.fileContent)
                }
            }.frame(width: geometry.size.width,alignment: .leading)
        }
    }
}

struct FileDetailView_Previews: PreviewProvider {
    static var previews: some View {
        FileDetailView(serverAddress: "", fileName: "")
    }
}
