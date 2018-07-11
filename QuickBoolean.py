import maya.cmds as cmds
import pymel.core as pm
import xml.dom.minidom as xd

#initialise
currentResultObj = cmds.ls(sl=True)
operandsGroupName = 'Operands'

#Window
cmds.window(title="Quick Boolean" )
cmds.columnLayout()
cmds.button( label="Union", c='QuickBoolean(1)')
cmds.button( label="Subtract", c='QuickBoolean(2)')
cmds.button( label="Intersect", c='QuickBoolean(3)')
cmds.button( label="New Start", c='QuickBoolean(4)')
cmds.button( label="Re-run", c='RerunOperation()')
cmds.showWindow()

#xml setup
root_node = None
xoperands = None
currentResult = None
doc = xd.Document()

#makes script work even with increments
path = cmds.file(q=True,exn=True)[:-3]
if "." in path:
    path = path[:-5]

xfile = None
xPath = path + '.xml'

#initialise objects
if cmds.file(xPath, q=True, ex=True) == True:
    doc = xd.parse(xPath)
    xcr = doc.getElementsByTagName("CurrentResult")[0].getAttribute("Name")
    currentResultObj = cmds.ls(xcr)
    print xcr  
else:
    startobj = cmds.duplicate(currentResultObj)
    cmds.select(currentResultObj, r=True)
    cmds.makeIdentity( apply=True, t=1, r=1, s=1, n=2 )
    cmds.select(operandsGroupName, r = True)
    cmds.parent(startobj, cmds.ls(sl=True))
    cmds.setAttr(startobj[0] + '.translateX', 0)
    cmds.setAttr(startobj[0] + '.translateY', 0)
    cmds.setAttr(startobj[0] + '.translateZ', 0)
    #create default nodes
    root_node = doc.createElement("Objects")
    doc.appendChild(root_node)
    xoperands = doc.createElement("Operands")
    root_node.appendChild(xoperands)
    operand = doc.createElement("Operand")
    operand.setAttribute(startobj[0], 1)
    currentResult = doc.createElement("CurrentResult")
    root_node.appendChild(currentResult)
    xmlFile = open(xPath, "w")
    xmlFile.write(doc.toprettyxml())
    xmlFile.close()
    doc = xd.parse(xPath)
    AddOperandToXml(startobj[0], 1, currentResultObj)

#checks if Operands exisits
if cmds.objExists(operandsGroupName) == False:
    grp = cmds.CreateEmptyGroup()
    cmds.rename(grp, operandsGroupName)

def QuickBoolean(operation):
    global currentResultObj
    obj = cmds.ls(sl=True)
    obj2 = cmds.duplicate(obj)
    cmds.select(obj2)
    cmds.makeIdentity( apply=True, t=1, r=1, s=1, n=2 )
    cmds.parent(obj2, operandsGroupName)
    #Moves the operand to the center of the group
    cmds.setAttr(obj2[0] + '.translateX', 0)
    cmds.setAttr(obj2[0] + '.translateY', 0)
    cmds.setAttr(obj2[0] + '.translateZ', 0)
    cmds.select(currentResultObj)
    if operation == 4:
        currentResultObj = obj
        AddOperandToXml(obj2[0], operation, currentResultObj)
        return
    cmds.polyCBoolOp( cmds.ls(sl=True), obj, op=operation, n='result')
    currentResultObj = cmds.ls(sl=True)
    cmds.delete(ch=True)
    AddOperandToXml(obj2[0], operation, currentResultObj)
    
def AddOperandToXml(name, operation, result):
    eResult = doc.getElementsByTagName("CurrentResult")[0]
    eResult.setAttribute("Name", result[0])
    element = doc.getElementsByTagName("Operands")[0]
    xop = doc.createElement("Operand")
    xop.setAttribute(name, str(operation))
    element.appendChild(xop)
    xmlFile = open(xPath, "w")
    xmlFile.write(doc.toprettyxml())
    xmlFile.close() 
    
def RerunOperation():
    operandsList = []
    for x in doc.getElementsByTagName("Operand"):
        attrs = x.attributes.keys()
        for a in attrs:
            keypair = x.attributes[a]
            operandsList.append((cmds.ls(keypair.name), int(keypair.value)))
    global currentResultObj
    currentResultObj = cmds.duplicate(operandsList[0][0])
    cmds.parent(currentResultObj, w=True)
    cmds.setAttr(currentResultObj[0] + '.translateX', 0)
    cmds.setAttr(currentResultObj[0] + '.translateY', 0)
    cmds.setAttr(currentResultObj[0] + '.translateZ', 0)
    for i in range(1, len(operandsList)):
        obj2 = cmds.duplicate(operandsList[i][0])
        cmds.parent(obj2, w=True)
        cmds.setAttr(obj2[0] + '.translateX', 0)
        cmds.setAttr(obj2[0] + '.translateY', 0)
        cmds.setAttr(obj2[0] + '.translateZ', 0)
        if operandsList[i][1] == 4:
            currentResultObj = obj2
            continue
        cmds.polyCBoolOp( currentResultObj, obj2, op=operandsList[i][1], n='result')
        currentResultObj = cmds.ls(sl=True)
        cmds.delete(ch=True)