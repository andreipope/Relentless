from .base import CZBTests
import time
class Horde_Editing_Page(CZBTests):

    def __init__(self,altdriver,driver):
        self.altdriver=altdriver
        self.driver=driver
        self.horde_editing_page=self.get_horde_editing_page()
        self.back_button=self.get_back_button()
        self.save_button=self.get_save_button()
        self.collection_area=self.get_collection_area()
        self.deck_area=self.get_deck_area()
        self.deck_name_input_field=self.get_deck_name_input_field()
        self.card_amount_text=self.get_card_amout_text()
        self.army_left_arrow_button=self.get_army_left_arrow()
        self.army_right_arrow_button=self.get_army_right_arrow()
        self.horde_left_arrow_button=self.get_horde_left_arrow()
        self.horde_right_arrow_button=self.get_horde_right_arrow()
        self.locator_collection_cards=self.get_locator_collection_cards()
        self.locator_deck_cards=self.get_locator_deck_cards()
    
    def get_horde_editing_page(self):
        return self.altdriver.wait_for_element('Tab_Editing')
    def get_back_button(self):
        return self.altdriver.wait_for_element('Image_ButtonBackTray/Button_Back')
    def get_save_button(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_FrameComponents/Lower_Items/Button_SaveDeck')
    def get_collection_area(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_Content/DragArea_Collections')
    def get_deck_area(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_Content/DragArea_Deck')
    def get_army_left_arrow(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_Content/Button_LowerLeftArrow')
    def get_army_right_arrow(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_Content/Button_LowerRightArrow')
    def get_horde_left_arrow(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_Content/Button_UpperLeftArrow')
    def get_horde_right_arrow(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_Content/Button_UpperRightArrow')

    def get_locator_collection_cards(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_Content/Locator_CollectionCards')
    def get_locator_deck_cards(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_Content/Locator_DeckCards')
    
    def get_deck_name_input_field(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_FrameComponents/Upper_Items/Text_DeckName')
    def get_card_amout_text(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Panel_FrameComponents/Lower_Items/Image_CardCounter/Text_CardsAmount')


    def press_army_left_arrow(self):
        self.button_pressed(self.army_left_arrow_button)
    def press_army_right_arrow(self):
        self.button_pressed(self.army_right_arrow_button)
    def press_horde_left_arrow(self):
        self.button_pressed(self.horde_left_arrow_button)
    def press_horde_right_arrow(self):
        self.button_pressed(self.horde_right_arrow_button)
    def press_save(self):
        self.button_pressed(self.save_button)
    def press_back(self):
        self.button_pressed(self.back_button)
    

    def get_cards_shown_in_collection_panel(self):
        cards=[]
        print(self.collection_area.name+'/Cards/CreatureCard(Clone)')
        cards = self.altdriver.find_elements(self.locator_collection_cards.name+'/CreatureCard(Clone)')
        cards.extend(self.altdriver.find_elements(self.locator_collection_cards.name+'/ItemCard(Clone)'))
        return cards
    def get_cards_shown_in_deck_panel(self):
        cards = self.altdriver.find_elements(self.locator_deck_cards.name+'/CreatureCard(Clone)')
        cards.extend(self.altdriver.find_elements(self.locator_deck_cards.name+'/ItemCard(Clone)'))
        return cards
    def get_card_name(self,card_id):
        card_name=self.altdriver.find_element('id('+card_id+')/TitleText')
        return self.read_tmp_GUI_text(card_name)
    def get_card_from_collection_panel(self,card_name):
        started_card_name=''
        cards=self.get_cards_shown_in_collection_panel()
        for card in cards:
            cardName=self.get_card_name(card.id)
            if cardName==card_name:
                return card
            started_card_name=cardName
        while True:
            print("Stop Check 3")

            self.press_army_right_arrow()
            cards=self.get_cards_shown_in_collection_panel()
            for card in cards:
                cardName=self.get_card_name(card.id)
                if cardName==card_name:
                    return card
                if started_card_name==card_name:
                    return None
    def can_card_be_added_to_horde(self,card):
        card_text_availability=self.altdriver.find_element('id('+card.id+')/AmountForArmy/Text')
        if not self.read_tmp_text(card_text_availability)=='0':
            return True
        return False

    def get_card_that_can_be_added_to_horde(self):
        while True:
            cards=[]
            print("Stop Check 2 ")

            cards=self.get_cards_shown_in_collection_panel()
            for card in cards:
                if self.can_card_be_added_to_horde(card):
                    return card
            self.press_army_right_arrow()
            time.sleep(1)
    def add_cards_to_horde(self,number_of_cards):
        for i in range(number_of_cards):
            print("Stop Check 1 with i= ",i)
            card=self.get_card_that_can_be_added_to_horde()
            self.driver.swipe(card.x,card.mobileY,self.deck_area.x,self.deck_area.mobileY,2000)


        



    
    


