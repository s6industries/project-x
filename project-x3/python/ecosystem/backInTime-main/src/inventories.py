import pygame
import os
import json
from . import weapons
import random

class Inventory:

    def __init__(self, player, screen, scale, pos, name):
        self.player = player
        self.screen = screen
        self.scale = scale
        self.name = name

        self.selected = 0
        self.guis = []
        self.weaponsgui = [] # for items gui viewing player's weapons / to craft items
        self.loadGUIs()
        self.rect = pygame.Rect(pos[0], pos[1], self.scale[0], self.scale[1])

        self.items = []

        # sfx
        self.sfx = []
        self.inv_sfx = None # for other sfx

        self.font = pygame.font.SysFont('arial', 15)
        self.message = None

    def draw(self):
        self.screen.blit(self.guis[self.selected], self.rect)

    def loadGUIs(self):
        path = f'characters/GUI/{self.name}'
        for filename in os.listdir(path):
            if filename.endswith('.png'):
                imag_path = os.path.join(path, filename)
                try:
                    image = pygame.image.load(imag_path)
                    image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
                    self.guis.append(image)
                except pygame.error:
                    print('file not found')

    def showLabel(self):
        try:
            if self.message == '' or self.message ==  None:
                if self.player.myWeapons[self.selected].player == None:
                    text = 'You are not the\nowner of this\nweapon'
                    text = self.font.render(text, True, (255,255,255))
                else:
                    text = self.player.myWeapons[self.selected]
                    text = self.font.render(f'Name: {text.name.title()}\nLevel: {text.level}\nDamage: {text.damage}\nAbsorb: {text.absorb}', True, (255, 255, 255))
            else: 
                text = self.font.render(self.message, True, (255,255,255))
        except IndexError:
            text = self.font.render(f'Name: Empty\nLevel: 0\nDamage: 0\nAbsorb: 0', True, (255,255,255))
        self.screen.blit(text, (210,110))

class Collectables(Inventory):

    def __init__(self, player, screen, scale, pos, name='collect'):
        super().__init__(player, screen, scale, pos, name)
        self.collectedGrid = []

        self.collectedItems = []
        self.collectedItemsCode = []
        
        self.column = 55
        self.row = 200
        self.createGrid()
        self.level = None

        self.dataWeapons = None
        self.openData()

    def draw(self):
        self.checkCollected() # check if all collectable items are already collected
        self.checkItems() # check for all collectable items in your inventories
        self.screen.blit(self.guis[0], self.rect)

        self.Message()
        self.screen.blit(self.message, (450, 200))
        self.drawCollected()

    def checkCollected(self): # if the player collected all the collectable items
        for data in self.dataWeapons:
            if data['level'] == self.level:
                if len(self.collectedItems) == len(data['collect']):
                    self.player.can_teleport = True # player can now teleport
                    if self.player.level > self.player.bossCount:
                        self.player.endgame = True
                    return True
        return False

    def Message(self):
        for data in self.dataWeapons:
            if data['level'] == self.level:
                self.message = f"Level: {self.level}\nCollected: {len(self.collectedItems)}\nRemaining: {len(data['collect']) - len(self.collectedItems)}"
                self.message = self.font.render(self.message, True, (255,255,255))

    def createGrid(self):
        for _ in range(2):
            for _ in range(6):
                self.collectedGrid.append((self.column, self.row))
                self.column += 65
            self.column = 55
            self.row += 65

    def checkItems(self):
        for data in self.dataWeapons:
            if data['level'] == self.level:
                for weapon in self.player.myWeapons:
                    if weapon.code in data['collect'] and weapon.code not in self.collectedItemsCode:
                        self.collectedItems.append(weapon)
                        self.collectedItemsCode.append(weapon.code)
                        self.player.myWeapons.remove(weapon)

    def openData(self):
        with open('data/items.json') as file:
            self.dataWeapons = json.load(file)

        self.dataWeapons = self.dataWeapons['Collectables']

    def Close(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_SPACE] or keys[pygame.K_f] or keys[pygame.K_ESCAPE]:
            return True

    def drawCollected(self):
        for i, weapon in enumerate(self.collectedItems):
            image = pygame.transform.scale(weapon.icon, (40, 40))
            self.screen.blit(image, self.collectedGrid[i])

class Items(Inventory):

    def __init__(self, player, screen, scale, pos, name='items_gui'):
        super().__init__(player, screen, scale, pos, name)
        self.chestGrid = []
        self.weaponGrid = []
        self.createGrid()

        self.name = 'items_gui'
        self.focus = 'chestGrid'
        self.selectedWeapons = 0

    def draw(self):
        if self.focus == 'chestGrid':
            self.screen.blit(self.guis[self.selected], self.rect)
        else:
            self.screen.blit(self.weaponsgui[self.selectedWeapons], self.rect)

    def loadGUIs(self): # temporary
        for i in range(20):
            image = f'characters/GUI/{self.name}/s_{i}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
            self.guis.append(image)

        for i in range(8):
            image = f'characters/GUI/{self.name}/e_{i}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
            self.weaponsgui.append(image)

    def createGrid(self):
        x1, y1 = 67, 134
        x2, y2 = 457, 165
        for _ in range(4):
            for _ in range(5):
                self.chestGrid.append((x1, y1))
                x1 += 70
            x1 = 67
            y1 += 70
        for _ in range(3):
            for _ in range(3):
                self.weaponGrid.append((x2, y2))
                x2 += 70
            x2 = 457
            y2 += 70

    def Select(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_TAB]:
            self.sfx[0].play()
            if self.focus == 'chestGrid':
                self.focus = 'weaponGrid'
            else:
                self.focus = 'chestGrid'

        if keys[pygame.K_RIGHT]:
            self.sfx[0].play()
            if self.focus == 'chestGrid':
                self.selected += 1 # increment selected

                if self.selected > (len(self.guis) -1): # if selected is greater than the number of boxes, reset to 0
                    self.selected = 0
            else:
                self.selectedWeapons += 1 # increment selected

                if self.selectedWeapons > (len(self.weaponsgui) -1): # if selected is greater than the number of boxes, reset to 0
                    self.selectedWeapons = 0

        elif keys[pygame.K_LEFT]:
            self.sfx[0].play()
            if self.focus == 'chestGrid':
                self.selected -= 1

                if self.selected < 0:
                    self.selected = len(self.guis) -1
            else:
                self.selectedWeapons -= 1 # increment selected

                if self.selectedWeapons < 0: # if selected is greater than the number of boxes, reset to 0
                    self.selectedWeapons = len(self.weaponsgui) -1

        elif keys[pygame.K_UP]:
            self.sfx[0].play()
            if self.focus == 'chestGrid':
                tmp = self.selected
                self.selected -= 5

                if self.selected < 0:
                    self.selected = tmp + 15

        elif keys[pygame.K_DOWN]:
            self.sfx[0].play()
            if self.focus == 'chestGrid':
                tmp = self.selected
                self.selected += 5

                if self.selected > len(self.guis) -1:
                    self.selected = tmp - 15

        # exit inventory
        elif keys[pygame.K_f] or keys[pygame.K_ESCAPE]:
            self.sfx[1].play()
            return True

        self.Move() # call function move
        
    def Move(self):
        keys = pygame.key.get_just_pressed()

        try:
            if keys[pygame.K_SPACE]:
                self.sfx[0].play()
                if self.focus == 'chestGrid': # move item from chestGrid to weaponGrid
                    # check if the weaponGrid is not yet full
                    if len(self.player.myWeapons) < 8:
                        self.player.myWeapons.append(self.player.inventories[self.selected]) # move the selected item to weaponGrid
                        self.player.inventories.remove(self.player.inventories[self.selected])
                else:
                    if len(self.player.inventories) < 20:
                        self.player.inventories.append(self.player.myWeapons[self.selectedWeapons])
                        self.player.myWeapons.remove(self.player.myWeapons[self.selectedWeapons])
        except IndexError:
            print('Empty')

    def drawInventories(self):
        try:
            # weapons
            for i, weapon in enumerate(self.player.myWeapons):
                wp = pygame.transform.scale(weapon.icon, (40,40))
                self.screen.blit(wp, self.weaponGrid[i])
            # items
            for i, item in enumerate(self.player.inventories):
                wp = pygame.transform.scale(item.icon, (40,40))
                self.screen.blit(wp, self.chestGrid[i])
        except IndexError:
            print('empty')


class Weapon(Inventory):

    def __init__(self, player, screen, scale, pos, name='weapons_gui'):
        super().__init__(player, screen, scale, pos, name)
        self.positions = [ # grid for all weapons and items - 8 items
            (225,280), (292,280), (358, 280), (424, 280),
            (225, 346), (292, 346), (358, 346), (424, 346)
            ]
        self.equiped = [(360, 195), (443, 195), (443, 108), (360, 108)] # for weapons equiped - 4 items

        self.switch = 0

        self.destroy_sfx = pygame.mixer.Sound('audio/shield_1.mp3')
        self.sfx_timer = 0

    def Select(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_TAB] or keys[pygame.K_RIGHT]:
            self.message = ''
            self.sfx[0].play()
            self.selected += 1 # increment selected

            if self.selected > (len(self.guis) -1): # if selected is greater than the number of boxes, reset to 0
                self.selected = 0

        elif keys[pygame.K_LEFT]:
            self.message = ''
            self.sfx[0].play()
            self.selected -= 1

            if self.selected < 0:
                self.selected = len(self.guis) -1

        elif keys[pygame.K_UP]:
            self.message = ''
            self.sfx[0].play()
            tmp = self.selected
            self.selected -= 4

            if self.selected < 0:
                self.selected = tmp + 4

        elif keys[pygame.K_DOWN]:
            self.message = ''
            self.sfx[0].play()
            tmp = self.selected
            self.selected += 4

            if self.selected > len(self.guis) -1:
                self.selected = tmp - 4

        # exit inventory
        elif keys[pygame.K_f] or keys[pygame.K_ESCAPE]:
            self.message = ''
            self.sfx[1].play()
            return True
        
        self.showLabel()
        self.SelectEquiped()
    
    def SelectEquiped(self):
        keys = pygame.key.get_just_pressed()

        try:
            # weapons
            if keys[pygame.K_SPACE]:
                self.sfx[1].play() # play sfx

                # weapon one
                if self.player.myWeapons[self.selected]._type not in ['potion', 'shield', 'item'] and not self.switch:
                    if self.player.myWeapons[self.selected].player == None:
                        self.message = f'You are not the\nowner of {self.player.myWeapons[self.selected].name}'
                    elif self.player.myWeapons[self.selected] != self.player.equiped2:
                        self.player.equiped1.effect = False # reset the effect
                        temp = self.player.equiped1
                        self.player.potion.Remove(temp)
                        self.player.equiped1 = self.player.myWeapons[self.selected] # equiped the weapon
                        self.player.myWeapons[self.selected] = temp
                        self.player.potion.Apply(self.player.equiped1)
                    else:
                        self.message = 'You can\'t use \nalready equiped\nweapon'

                    self.switch = 1

                # weapon two
                elif self.player.myWeapons[self.selected]._type not in ['potion', 'shield', 'item'] and self.switch: 
                    if self.player.myWeapons[self.selected].player == None:
                        self.message = f'You are not the\nowner of {self.player.myWeapons[self.selected].name}'
                    elif self.player.myWeapons[self.selected] != self.player.equiped1:
                        self.player.equiped2.effect = False # reset the effect
                        temp = self.player.equiped2
                        self.player.potion.Remove(temp)
                        self.player.equiped2 = self.player.myWeapons[self.selected]
                        self.player.myWeapons[self.selected] = temp
                        self.player.potion.Apply(self.player.equiped2)
                    else:
                        self.message = 'You can\'t use \nalready equiped\nweapon'

                    self.switch = 0

                # potion
                elif self.player.myWeapons[self.selected]._type not in ['shield', 'weapon', 'weapon-shield', 'item']:
                    self.player.potion.effect = False # reset the effect
                    temp = self.player.potion
                    self.player.potion.Remove(self.player.equiped1) # reset
                    self.player.potion.Remove(self.player.equiped2) # reset
                    self.player.potion.Remove(self.player.shield) # reset
                    self.player.potion = self.player.myWeapons[self.selected]
                    self.player.myWeapons[self.selected] = temp # return to inventory
                    self.player.potion.Apply(self.player.equiped1) # apply
                    self.player.potion.Apply(self.player.equiped2) # apply
                    self.player.potion.Apply(self.player.shield) # apply

                # shield
                elif self.player.myWeapons[self.selected]._type not in ['potion', 'weapon', 'item']:
                    self.player.shield.effect = False # reset the effect
                    temp = self.player.shield
                    self.player.potion.Remove(temp)
                    self.player.shield = self.player.myWeapons[self.selected]
                    self.player.myWeapons[self.selected] = temp
                    self.player.potion.Apply(self.player.shield)

            # remove item
            elif keys[pygame.K_r]:
                self.destroy_sfx.play()
                addLife = random.choice([5,10,15,20,25,30])

                # if the item is a weapon or not
                if self.player.myWeapons[self.selected]._type in ['weapon', 'shield', 'potion']:
                    self.player.life = self.player.defaultLife
                else:
                    if self.player.life <= (self.player.defaultLife - addLife):
                        self.player.life += addLife
                    else:
                        self.player.life = self.player.defaultLife

                self.message = f'{self.player.myWeapons[self.selected].name.title()} removed\nHealth: +{addLife}'
                self.player.myWeapons.remove(self.player.myWeapons[self.selected])

        except IndexError:
            print('select empty')
    
    def drawWeapons(self):
        eq1 = pygame.transform.scale(self.player.equiped1.icon, (45, 45))
        eq2 = pygame.transform.scale(self.player.equiped2.icon, (45, 45))
        eq_S = pygame.transform.scale(self.player.shield.icon, (45, 45))
        eq_P = pygame.transform.scale(self.player.potion.icon, (45, 45))
        self.screen.blit(eq1, self.equiped[0]) # left mouse
        self.screen.blit(eq2, self.equiped[1]) # right mouse
        self.screen.blit(eq_S, self.equiped[2]) # shield
        self.screen.blit(eq_P, self.equiped[3]) # potions

        for weapon in range(len(self.player.myWeapons)):
            wp = pygame.transform.scale(self.player.myWeapons[weapon].icon, (45, 45))
            self.screen.blit(wp, self.positions[weapon])

class CraftingTable(Inventory):

    def __init__(self, player, screen, scale, pos, name='crafting_table'):
        super().__init__(player, screen, scale, pos, name)
        self.placed = []
        self.placed_code = []
        self.result = None

        self.itemsGrid = [
            (120, 252), (180, 252), (240, 252), (300, 252),
            (120, 312), (180, 312), (240, 312), (300, 312)
            ]
        self.craftGrid = [
            (424, 164), (484, 164),
            (424, 224), (484, 224)
        ]
        self.resultGrid = (456, 300)
        self.focus = 'myWeapons'
        self.selectedCraft = 0

        self.dataCombination = []
        self.getCombinationData()

        self.inv_sfx = pygame.mixer.Sound('audio/crafted.mp3')
        self.sfx_timer = 0

    def Select(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_LEFT]:
            self.sfx[0].play()
            if self.focus == 'myWeapons':
                self.selected -= 1

                if self.selected < 0:
                    self.selected = len(self.guis) -1
            else:
                self.selectedCraft -= 1

                if self.selectedCraft < 0:
                    self.selectedCraft = len(self.weaponsgui) -1

        elif keys[pygame.K_RIGHT]:
            self.sfx[0].play()
            if self.focus == 'myWeapons':
                self.selected += 1

                if self.selected > len(self.guis) -1:
                    self.selected = 0
            else:
                self.selectedCraft += 1

                if self.selectedCraft > len(self.weaponsgui) -1:
                    self.selectedCraft = 0

        elif keys[pygame.K_UP]:
            self.sfx[0].play()
            if self.focus == 'myWeapons':
                tmp = self.selected
                self.selected -= 4

                if self.selected < 0:
                    self.selected = tmp + 4
            else:
                temp = self.selectedCraft
                self.selectedCraft -= 2

                if self.selectedCraft < 0:
                    self.selectedCraft = temp + 2

        elif keys[pygame.K_DOWN]:
            self.sfx[0].play()
            if self.focus == 'myWeapons':
                tmp = self.selected
                self.selected += 4

                if self.selected > len(self.guis) - 1:
                    self.selected = tmp - 4
            else:
                temp = self.selectedCraft
                self.selectedCraft += 2

                if self.selectedCraft > len(self.weaponsgui) -1:
                    self.selectedCraft = temp - 2

        elif keys[pygame.K_TAB]:
            self.sfx[0].play()
            if self.focus == 'myWeapons':
                self.focus = 'items'
            else:
                self.focus = 'myWeapons'
        try:
            if keys[pygame.K_SPACE]:
                self.sfx[1].play()
                if self.focus == 'myWeapons':
                    if len(self.placed) < 4:
                        self.placed.append(self.player.myWeapons[self.selected])
                        self.placed_code.append(self.player.myWeapons[self.selected].code)
                        self.player.myWeapons.remove(self.player.myWeapons[self.selected])
                else:
                    if len(self.player.myWeapons) < 8:
                        self.player.myWeapons.append(self.placed[self.selectedCraft])
                        self.placed.remove(self.placed[self.selectedCraft])
                        self.placed_code.remove(self.placed_code[self.selectedCraft])
        
        except IndexError:
            print('empty')

        if keys[pygame.K_f] or keys[pygame.K_ESCAPE]:
            self.sfx[1].play()
            self.result = None
            return True
        
        self.Craft()
        
    def draw(self):
        if self.focus == 'myWeapons':
            self.screen.blit(self.guis[self.selected], self.rect)
        else:
            self.screen.blit(self.weaponsgui[self.selectedCraft], self.rect)
        
    def drawItems(self):
        try:
            for i, item in enumerate(self.player.myWeapons): # draw all weapons ang items
                image = pygame.transform.scale(item.icon, (40, 40))
                self.screen.blit(image, self.itemsGrid[i])

            for i, item in enumerate(self.placed): # draw the item that placed in crafting table
                image = pygame.transform.scale(item.icon, (40, 40))
                self.screen.blit(image, self.craftGrid[i])

            if self.result != None:
                image = pygame.transform.scale(self.result.icon, (40,40))
                self.screen.blit(image, self.resultGrid)
                
        except IndexError:
            print('empty')

    def loadGUIs(self): # temporary
        for i in range(8):
            image = f'characters/GUI/{self.name}/s_{i}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
            self.guis.append(image)

        for i in range(4):
            image = f'characters/GUI/{self.name}/e_{i}.png'
            image = pygame.image.load(image)
            image = pygame.transform.scale(image, (self.scale[0], self.scale[1]))
            self.weaponsgui.append(image)

    def Craft(self):
        for data in self.dataCombination['combination']: # load all combination data
            if self.placed_code == data['combination']: # if the items in placed items math in combination data
                # play the sfx 
                if self.sfx_timer == 0:
                    self.sfx_timer = 10
                    self.inv_sfx.play()

                # build the item
                self.build(data['result'], data['type'])
                self.placed_code = []
                self.placed = []
        
        # sfx timer
        if self.sfx_timer > 0:
            self.sfx_timer -= 1

    def getCombinationData(self):
        with open('data/items.json') as file:
            self.dataCombination = json.load(file)

    def build(self, name, _type='item'):
        # weapon = None
        if _type == 'weapon':
            if name == 'boomerang':
                weapon = weapons.Boomerang(self.player, self.screen, (30,30))
            elif name == 'bomb':
                weapon = weapons.Bomb(self.player, self.screen, (30,30))
            self.player.myWeapons.append(weapon)
            self.result = weapon
        else:
            # create item
            item = weapons.Items(self.player, self.screen, (40, 40), name)

            # check the item
            if item.checkItem():
                self.player.myWeapons.append(item)
                self.result = item